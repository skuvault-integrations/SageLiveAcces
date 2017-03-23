using System.Threading.Tasks;
using Netco.Monads;
using SageLiveAccess.Misc;
using SageLiveAccess.sforce;

namespace SageLiveAccess.Helpers
{
	internal class DocumentTypeHelper
	{
		private readonly AsyncQueryManager _asyncQueryManager;

		public DocumentTypeHelper( AsyncQueryManager asyncQueryManager )
		{
			this._asyncQueryManager = asyncQueryManager;
		}

		public async Task< Maybe< s2cor__Sage_INV_Trade_Document_Type__c > > GetSaleInvoiceTypeId()
		{
			var result = await this._asyncQueryManager.QueryOneAsync< s2cor__Sage_INV_Trade_Document_Type__c >( SoqlQuery.Builder().Select( "Id", "Name" ).From( "s2cor__Sage_INV_Trade_Document_Type__c" ).Where( "Name" ).IsEqualTo( "Sales Invoice" ) );
			return result;
		}

		public async Task< s2cor__Sage_ACC_Dimension__c > GetSaleInvoiceDimensionTypeId()
		{
			return ( await this._asyncQueryManager.QueryOneAsync< s2cor__Sage_ACC_Dimension__c >( SoqlQuery.Builder().Select( "Id", "Name" ).From( "s2cor__Sage_ACC_Dimension__c" ).Where( "Name" ).IsEqualTo( "Sales Invoice Number" ) ) ).Value;
		}

		public async Task< s2cor__Sage_ACC_Dimension__c > GetPurchaseInvoiceDimensionTypeId()
		{
			return ( await this._asyncQueryManager.QueryOneAsync< s2cor__Sage_ACC_Dimension__c >( SoqlQuery.Builder().Select( "Id", "Name" ).From( "s2cor__Sage_ACC_Dimension__c" ).Where( "Name" ).IsEqualTo( "Purchase Order Number" ) ) ).Value;
		}

		public async Task< s2cor__Sage_INV_Trade_Document_Type__c > GetPurchaseInvoiceTypeId()
		{
			return ( await this._asyncQueryManager.QueryOneAsync< s2cor__Sage_INV_Trade_Document_Type__c >( SoqlQuery.Builder().Select( "Id", "Name" ).From( "s2cor__Sage_INV_Trade_Document_Type__c" ).Where( "Name" ).IsEqualTo( "Purchase Order" ) ) ).Value;
		}
	}
}