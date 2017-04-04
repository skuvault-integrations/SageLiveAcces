using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Netco.Extensions;
using SageLiveAccess.Helpers;
using SageLiveAccess.Misc;
using SageLiveAccess.Models;
using SageLiveAccess.Models.Auth;
using SageLiveAccess.sforce;
using Task = System.Threading.Tasks.Task;

namespace SageLiveAccess.Services
{
	internal class PushInvoiceService: MethodLogging
	{
		private readonly AsyncQueryManager _asyncQueryManager;
		private readonly SageLiveAuthInfo _authInfo;
		private readonly PaginationManager _paginationManager;
		private readonly PushInvoiceItemHelper _invoiceItemHelper;
		private readonly PushInvoiceHelper _invoiceHelper;
		private readonly DocumentTypeHelper _documentTypeHelper;
		private readonly CurrencyHelper _currencyHelper;

		const string ServiceName = "PullInvoiceService";

		public PushInvoiceService( AsyncQueryManager asyncQueryManager, PaginationManager paginationManager, SageLiveAuthInfo authInfo, SageLivePushInvoiceSettings pushInvoiceSettings )
		{
			this._asyncQueryManager = asyncQueryManager;
			this._authInfo = authInfo;
			this._paginationManager = paginationManager;
			this._invoiceItemHelper = new PushInvoiceItemHelper( asyncQueryManager, paginationManager, authInfo );
			this._invoiceHelper = new PushInvoiceHelper( asyncQueryManager, authInfo, pushInvoiceSettings );
			this._currencyHelper = new CurrencyHelper( this._asyncQueryManager );
			this._documentTypeHelper = new DocumentTypeHelper( this._asyncQueryManager );
		}

		private async Task PushTransactionItems( IEnumerable< InvoiceBase > saleInvoices, string[] saleInvoicesCreated, Dictionary< string, string > existingProducts, Mark mark, CancellationToken ct )
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

			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, mark, ServiceName ), "Pushing transaction items: {0} ".FormatWith( transactionItems.MakeString() ) );
			await this._paginationManager.InsertAll( transactionItems, mark, ct );
		}

		private async Task CreateNewInvoices( IEnumerable< InvoiceBase > saleInvoices, string salesInvoiceDocumentTypeId, string currencyId, string dimensionId, Mark mark, CancellationToken ct )
		{
			var presentAndAbsentProductInfo = await this._invoiceItemHelper.GetPresentAndAbsentProductInfo( saleInvoices, mark, ct );
			var existingProducts = presentAndAbsentProductInfo.existingProducts;

			await this._invoiceItemHelper.CreateAbsentProducts( presentAndAbsentProductInfo, mark, ct );

			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, mark, ServiceName ), "Creating new invoices: {0} ".FormatWith( saleInvoices.MakeString() ) );
			var saleInvoicesCreated = ( await this._paginationManager.InsertAll( await this._invoiceHelper.CreateSaleInvoices( saleInvoices, salesInvoiceDocumentTypeId, currencyId, dimensionId, mark, ct ), mark, ct ) ).ToArray();

			await this.PushTransactionItems( saleInvoices, saleInvoicesCreated, existingProducts, mark, ct );
		}

		private async Task UpdateExistingInvoices( IEnumerable< KeyValuePair< InvoiceBase, string > > saleInvoicesInfo, string salesInvoiceDocumentTypeId, string currencyId, string dimensionId, Mark mark, CancellationToken ct )
		{
			foreach( var invoiceKv in saleInvoicesInfo )
			{
				await this._invoiceItemHelper.DeleteOldTransactionItems( invoiceKv.Value, mark, ct );
			}

			var saleInvoices = saleInvoicesInfo.Select( x => x.Key );

			var presentAndAbsentProductInfo = await this._invoiceItemHelper.GetPresentAndAbsentProductInfo( saleInvoices, mark, ct );
			var existingProducts = presentAndAbsentProductInfo.existingProducts;

			await this._invoiceItemHelper.CreateAbsentProducts( presentAndAbsentProductInfo, mark, ct );

			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, mark, ServiceName ), "Updating existing invoices: {0} ".FormatWith( saleInvoices.MakeString() ) );
			var saleInvoicesCreated = ( await this._paginationManager.UpdateAll( await this._invoiceHelper.CreateSaleInvoices( saleInvoices, salesInvoiceDocumentTypeId, currencyId, dimensionId, mark, ct ), mark, ct ) ).ToArray();

			await this.PushTransactionItems( saleInvoices, saleInvoicesInfo.Select( x => x.Value ).ToArray(), existingProducts, mark, ct );
		}

		private async Task PushInvoices( IEnumerable< InvoiceBase > saleInvoices, string currecyCode, string invoiceTypeId, string dimemsionId, Mark mark, CancellationToken ct )
		{
			var currencyId = ( await this._currencyHelper.GetCurrencyByCode( currecyCode, mark, ct ) ).Id;

			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, mark, ServiceName ), "Mark:{0}. Processing invoices for further creating or updating: {1}.".FormatWith( mark, saleInvoices.MakeString() ) );
			var invoiceInfo = await this._invoiceHelper.GetPresentAndAbsentInvoiceInfo( saleInvoices, mark, ct );
			await this.CreateNewInvoices( invoiceInfo._invoicesToCreate, invoiceTypeId, currencyId, dimemsionId, mark, ct );
			// off for now
			//			await this.UpdateExistingInvoices( invoiceInfo._invoicesToUpdate, salesInvoiceDocumentTypeId, currencyId );
		}

		public async Task PushSaleInvoices( IEnumerable< SaleInvoice > saleInvoices, string currecyCode, Mark mark, CancellationToken ct )
		{
			var invoiceDocumentTypeId = ( await this._documentTypeHelper.GetSaleInvoiceTypeId( mark, ct ) ).Value.Id;
			var dimensionId = ( await this._documentTypeHelper.GetSaleInvoiceDimensionTypeId( mark, ct ) ).Id;
			await this.PushInvoices( saleInvoices, currecyCode, invoiceDocumentTypeId, dimensionId, mark, ct );
		}

		public async Task PushPurchaseInvoices( IEnumerable< PurchaseInvoice > saleInvoices, string currecyCode, Mark mark, CancellationToken ct )
		{
			var invoiceDocumentTypeId = ( await this._documentTypeHelper.GetPurchaseInvoiceTypeId( mark, ct ) ).Id;
			var dimensionId = ( await this._documentTypeHelper.GetPurchaseInvoiceDimensionTypeId( mark, ct ) ).Id; // will not be used for now
			await this.PushInvoices( saleInvoices, currecyCode, invoiceDocumentTypeId, dimensionId, mark, ct );
		}
	}
}