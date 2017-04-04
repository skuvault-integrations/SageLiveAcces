using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SageLiveAccess.Misc;
using SageLiveAccess.Models;
using SageLiveAccess.Models.Auth;
using SageLiveAccess.sforce;

namespace SageLiveAccess
{
	class SageLiveSettingsService: ISageLiveSettingServicecs
	{
		private readonly PaginationManager paginationManager;

		public SageLiveSettingsService( SageLiveAuthInfo authInfo, SageLiveFactoryConfig config )
		{
			var service = SaleForceConnectionCreator.CreateSforceService( authInfo );
			var asyncQueryManager = new AsyncQueryManager( service, config, authInfo._refreshToken._refreshToken );
			this.paginationManager = new PaginationManager( asyncQueryManager );
		}

		public async Task< SageLiveLegislationInfo > GetLegislationInfo( CancellationToken ct )
		{
			var mark = Mark.CreateNew();
			SageLiveLogger.LogStarted( mark, string.Empty );

			var legislations = await this.paginationManager.GetAll< s2cor__Sage_COR_Legislation__c >( SoqlQuery.Builder().Select( "Id", "Name" ).From( "s2cor__Sage_COR_Legislation__c" ), mark, ct /*"SELECT Id, Name FROM s2cor__Sage_COR_Legislation__c"*/ ).ConfigureAwait( false );
			var result = new SageLiveLegislationInfo
			{
				legislations = legislations.ToDictionary( kv => kv.Id, kv => kv.Name )
			};

			SageLiveLogger.LogEnd( mark, string.Empty, legislations.MakeString() );
			return result;
		}

		public async Task< SageLiveInvoiceAccountInfo > GetInvoiceAccountInfo( CancellationToken ct )
		{
			var mark = Mark.CreateNew();
			SageLiveLogger.LogStarted( mark, string.Empty );

			var currencies = await this.paginationManager.GetAll< Account >( SoqlQuery.Builder().Select( "Id", "Name" ).From( "Account" ), mark, ct );
			var result = new SageLiveInvoiceAccountInfo
			{
				invoiceAccounts = currencies.ToDictionary( kv => kv.Id, kv => kv.Name )
			};

			SageLiveLogger.LogEnd( mark, string.Empty, currencies.MakeString() );
			return result;
		}
	}
}