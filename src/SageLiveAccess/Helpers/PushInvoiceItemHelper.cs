using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Netco.Extensions;
using Netco.Monads;
using SageLiveAccess.Misc;
using SageLiveAccess.Models;
using SageLiveAccess.Models.Auth;
using SageLiveAccess.sforce;
using Task = System.Threading.Tasks.Task;

namespace SageLiveAccess.Helpers
{
	internal class PresentAndAbsentProductInfo
	{
		public readonly Dictionary< string, string > existingProducts = new Dictionary< string, string >();
		public readonly List< sObject > productsToCreate = new List< sObject >();
	}

	internal class PushInvoiceItemHelper : MethodLogging
	{
		private readonly AsyncQueryManager _asyncQueryManager;
		private readonly PaginationManager _paginationManager;
		private readonly SageLiveAuthInfo _authInfo;

		private const string ServiceName = "PushInvoiceItemsHelper";

		public PushInvoiceItemHelper( AsyncQueryManager asyncQueryManager, PaginationManager paginationManager, SageLiveAuthInfo authInfo )
		{
			this._asyncQueryManager = asyncQueryManager;
			this._paginationManager = paginationManager;
			this._authInfo = authInfo;
		}

		public async Task< Maybe< Product2 > > GetProductInfo( string sku )
		{
			return await this._asyncQueryManager.QueryOneAsync< Product2 >( SoqlQuery.Builder().Select( "Id" ).From( "Product2" ).Where( "ProductCode" ).IsEqualTo( sku ) );
		}

		public sObject CreateProduct( InvoiceItem item )
		{
			var product = new Product2();
			product.Name = item.ProductName;
			product.ProductCode = item.ProductCode;

			return product;
		}

		public sObject CreateTransactionItem( string invoiceId, string productId, double quantity, double price )
		{
			var item = new s2cor__Sage_INV_Trade_Document_Item__c();
			item.s2cor__Product__c = productId;
			item.s2cor__Quantity__c = quantity;
			item.s2cor__Unit_Price__c = price;
			item.s2cor__Trade_Document__c = invoiceId;
			item.s2cor__Quantity__cSpecified = true;
			item.s2cor__Unit_Price__cSpecified = true;
			return item;
		}

		public async Task< PresentAndAbsentProductInfo > GetPresentAndAbsentProductInfo( IEnumerable< InvoiceBase > saleInvoices )
		{
			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, ServiceName ), "Getting present and absent products for push selection..." );
			var result = new PresentAndAbsentProductInfo();

			var items = saleInvoices.SelectMany( saleInvoice => saleInvoice.Items );

			foreach( var item in items )
			{
				var productInfo = await this.GetProductInfo( item.ProductCode );
				if( productInfo.HasValue )
					result.existingProducts[ item.ProductCode ] = productInfo.Value.Id;
				else
					result.productsToCreate.Add( this.CreateProduct( item ) );
			}
			return result;
		}

		public async Task CreateAbsentProducts( PresentAndAbsentProductInfo productInfo )
		{
			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, ServiceName ), "Creating products: {0}".FormatWith( productInfo.productsToCreate.MakeString() ) );
			var productsCreated = ( await this._paginationManager.InsertAll( productInfo.productsToCreate ) ).ToArray();
			var productsToCreateArr = productInfo.productsToCreate.ToArray();

			for( int i = 0; i < productsCreated.Length; i++ )
			{
				productInfo.existingProducts[ ( ( Product2 )productsToCreateArr[ i ] ).ProductCode ] = productsCreated[ i ];
			}
		}

		public async Task DeleteOldTransactionItems( string invoiceId )
		{
			var ids = await this._paginationManager.GetAll< s2cor__Sage_INV_Trade_Document_Item__c >( SoqlQuery.Builder().Select( "Id" ).From( "s2cor__Sage_INV_Trade_Document_Item__c" ).Where( "s2cor__Trade_Document__c" ).IsEqualTo( invoiceId )  /* "SELECT Id FROM s2cor__Sage_INV_Trade_Document_Item__c WHERE s2cor__Trade_Document__c = '{0}'".FormatWith( invoiceId ) */);
			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, ServiceName ), "Deletting all the transactions items for invoice #{0}. Deleted ids: {1}".FormatWith( invoiceId, ids.MakeString() )  );
			await this._asyncQueryManager.Delete( ids.Select( x => x.Id ).ToArray() );
		}

	}
}