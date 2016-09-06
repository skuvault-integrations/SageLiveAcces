using System.Threading.Tasks;
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

		public async Task<s2cor__Sage_INV_Trade_Document_Type__c> GetSaleInvoiceTypeId()
		{
			return ( await this._asyncQueryManager.QueryOneAsync<s2cor__Sage_INV_Trade_Document_Type__c>( SoqlQuery.Builder().Select( "Id", "Name" ).From( "s2cor__Sage_INV_Trade_Document_Type__c" ).Where( "Name" ).IsEqualTo( "Sales Invoice" ) ) ).Value;
		}

		public async Task<s2cor__Sage_INV_Trade_Document_Type__c> GetPurchaseInvoiceTypeId()
		{
			return ( await this._asyncQueryManager.QueryOneAsync<s2cor__Sage_INV_Trade_Document_Type__c>( SoqlQuery.Builder().Select( "Id", "Name" ).From( "s2cor__Sage_INV_Trade_Document_Type__c" ).Where( "Name" ).IsEqualTo( "Purchase Invoice" ) ) ).Value;
		}

	}
}
