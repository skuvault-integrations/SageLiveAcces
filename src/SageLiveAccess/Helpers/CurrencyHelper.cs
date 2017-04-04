using System.Threading;
using System.Threading.Tasks;
using Netco.Monads;
using SageLiveAccess.Misc;
using SageLiveAccess.sforce;

namespace SageLiveAccess.Helpers
{
	internal class CurrencyHelper
	{
		private readonly AsyncQueryManager _asyncQueryManager;

		private const string DefaultCurrency = "USD";

		public CurrencyHelper( AsyncQueryManager asyncQueryManager )
		{
			this._asyncQueryManager = asyncQueryManager;
		}

		private async Task< Maybe< s2cor__Sage_COR_Currency__c > > GetCurrency( string code, Mark mark, CancellationToken ct )
		{
			return await this._asyncQueryManager.QueryOneAsync< s2cor__Sage_COR_Currency__c >( SoqlQuery.Builder().Select( "Id" ).From( "s2cor__Sage_COR_Currency__c" ).Where( "s2cor__Currency_Code__c" ).IsEqualTo( code ), mark, ct );
		}

		public async Task< s2cor__Sage_COR_Currency__c > GetCurrencyByCode( string currencyCode, Mark mark, CancellationToken ct )
		{
			return ( await this.GetCurrency( currencyCode, mark, ct ) ).GetValue( ( await this.GetCurrency( DefaultCurrency, mark, ct ) ).Value );
		}
	}
}
