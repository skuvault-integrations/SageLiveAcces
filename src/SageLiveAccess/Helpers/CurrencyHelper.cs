using System.Threading.Tasks;
using Netco.Monads;
using SageLiveAccess.Misc;
using SageLiveAccess.Models;
using SageLiveAccess.sforce;

namespace SageLiveAccess.Helpers
{
    internal class CurrencyHelper
    {
        private readonly PaginationManager _paginationManager;
        private readonly AsyncQueryManager _asyncQueryManager;

	    private const string DefaultCurrency = "USD";

		public CurrencyHelper( PaginationManager paginationManager, AsyncQueryManager asyncQueryManager )
        {
            this._paginationManager = paginationManager;
            this._asyncQueryManager = asyncQueryManager;
        }

		private async Task<Maybe< s2cor__Sage_COR_Currency__c> > GetCurrency( string code )
		{
			return await this._asyncQueryManager.QueryOneAsync<s2cor__Sage_COR_Currency__c>( SoqlQuery.Builder().Select( "Id" ).From( "s2cor__Sage_COR_Currency__c" ).Where( "s2cor__Currency_Code__c" ).IsEqualTo( code ) );
		}

        public async Task< s2cor__Sage_COR_Currency__c > GetCurrencyByCode( string currencyCode )
        {
	        return ( await this.GetCurrency( currencyCode ) ).GetValue( ( await this.GetCurrency( DefaultCurrency ) ).Value );
        }
    }
}
