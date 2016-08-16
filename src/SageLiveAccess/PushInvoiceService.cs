using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Netco.Extensions;
using Netco.Monads;
using SageLiveAccess.Misc;
using SageLiveAccess.Models;
using SageLiveAccess.Models.Auth;
using SageLiveAccess.sforce;
using Task = System.Threading.Tasks.Task;

namespace SageLiveAccess
{
	internal class PushInvoiceService : MethodLogging
	{
		private readonly AsyncQueryManager _asyncQueryManager;
		private readonly SageLiveAuthInfo _authInfo;
		private readonly PaginationManager _paginationManager;
		private readonly PushInvoiceItemHelper _invoiceItemHelper;
		private readonly PushInvoiceHelper _invoiceHelper;
        private readonly CurrencyHelper _currencyHelper;

		const string ServiceName = "PullInvoiceService";

		public PushInvoiceService( AsyncQueryManager asyncQueryManager, PaginationManager paginationManager, SageLiveAuthInfo authInfo, SageLivePushInvoiceSettings pushInvoiceSettings )
		{
			this._asyncQueryManager = asyncQueryManager;
			this._authInfo = authInfo;
			this._paginationManager = paginationManager;
			this._invoiceItemHelper = new PushInvoiceItemHelper( asyncQueryManager, paginationManager, authInfo );
			this._invoiceHelper = new PushInvoiceHelper( asyncQueryManager, paginationManager, authInfo, pushInvoiceSettings );
            this._currencyHelper = new CurrencyHelper( this._paginationManager, this._asyncQueryManager );
		}

		private async Task< s2cor__Sage_INV_Trade_Document_Type__c > GetSaleInvoiceTypeId()
		{
			return ( await this._asyncQueryManager.QueryOneAsync< s2cor__Sage_INV_Trade_Document_Type__c >( SoqlQuery.Builder().Select( "Id", "Name" ).From( "s2cor__Sage_INV_Trade_Document_Type__c" ).Where( "Name" ).IsEqualTo( "Sales Invoice" ) ) ).Value;
		}

		private async Task PushTransactionItems( IEnumerable< SaleInvoice > saleInvoices, string[] saleInvoicesCreated, Dictionary< string, string > existingProducts )
		{			
			var saleInvoicesArr = saleInvoices.ToArray();
			var transactionItems = new List< sObject >();

			for( int i = 0; i < saleInvoicesCreated.Length; i++ )
			{
				saleInvoicesArr[ i ].Items.ForEach( x =>
				{
					transactionItems.Add( this._invoiceItemHelper.CreateTransactionItem( saleInvoicesCreated[ i ], existingProducts[ x.ProductCode ], x.Quantity, x.UnitPrice ) );
				} );
			}

			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, ServiceName ), "Pushing transaction items: {0} ".FormatWith( transactionItems.MakeString() ) );
			await this._paginationManager.InsertAll( transactionItems );
		}

		private async Task CreateNewInvoices( IEnumerable< SaleInvoice > saleInvoices, string salesInvoiceDocumentTypeId, string currencyId )
		{
			var presentAndAbsentProductInfo = await this._invoiceItemHelper.GetPresentAndAbsentProductInfo( saleInvoices );
			var existingProducts = presentAndAbsentProductInfo.existingProducts;

			await this._invoiceItemHelper.CreateAbsentProducts( presentAndAbsentProductInfo );

			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, ServiceName ), "Creating new invoices: {0} ".FormatWith( saleInvoices.MakeString() ) );
			var saleInvoicesCreated = ( await this._paginationManager.InsertAll( await this._invoiceHelper.CreateSaleInvoices( saleInvoices, salesInvoiceDocumentTypeId, currencyId ) ) ).ToArray();

			await this.PushTransactionItems( saleInvoices, saleInvoicesCreated, existingProducts );
		}

		private async Task UpdateExistingInvoices( IEnumerable< KeyValuePair< SaleInvoice, string > > saleInvoicesInfo, string salesInvoiceDocumentTypeId, string currencyId )
		{
			foreach( var invoiceKv in saleInvoicesInfo )
			{
				await this._invoiceItemHelper.DeleteOldTransactionItems( invoiceKv.Value );
			}

			var saleInvoices = saleInvoicesInfo.Select( x => x.Key );

			var presentAndAbsentProductInfo = await this._invoiceItemHelper.GetPresentAndAbsentProductInfo( saleInvoices );
			var existingProducts = presentAndAbsentProductInfo.existingProducts;

			await this._invoiceItemHelper.CreateAbsentProducts( presentAndAbsentProductInfo );

			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, ServiceName ), "Updating existing invoices: {0} ".FormatWith( saleInvoices.MakeString() ) );
			var saleInvoicesCreated = ( await this._paginationManager.UpdateAll( await this._invoiceHelper.CreateSaleInvoices( saleInvoices, salesInvoiceDocumentTypeId, currencyId ) ) ).ToArray();

			await this.PushTransactionItems( saleInvoices, saleInvoicesInfo.Select( x => x.Value ).ToArray(), existingProducts );
		}

		public async Task PushSaleInvoices( IEnumerable< SaleInvoice > saleInvoices, string currecyCode, CancellationToken ct )
		{
			var salesInvoiceDocumentTypeId = ( await this.GetSaleInvoiceTypeId() ).Id;
            var currencyId = ( await this._currencyHelper.GetCurrencyByCode( currecyCode ) ).Value.Id;

			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, ServiceName ), "Processing invoices for further creating or updating: {0} ".FormatWith( saleInvoices.MakeString() ) );
			var invoiceInfo = await this._invoiceHelper.GetPresentAndAbsentInvoiceInfo( saleInvoices );
			await this.CreateNewInvoices( invoiceInfo._invoicesToCreate, salesInvoiceDocumentTypeId, currencyId );
            // off for now
//			await this.UpdateExistingInvoices( invoiceInfo._invoicesToUpdate, salesInvoiceDocumentTypeId, currencyId );
		}
	}
}