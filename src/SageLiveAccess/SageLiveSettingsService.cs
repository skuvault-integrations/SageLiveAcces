using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var legislations = await this.paginationManager.GetAll< s2cor__Sage_COR_Legislation__c >( SoqlQuery.Builder().Select( "Id", "Name" ).From( "s2cor__Sage_COR_Legislation__c" ) /*"SELECT Id, Name FROM s2cor__Sage_COR_Legislation__c"*/ ).ConfigureAwait( false );
            return new SageLiveLegislationInfo
            {
                legislations = legislations.ToDictionary( kv => kv.Id, kv => kv.Name )
            };
        }

        public async Task< SageLiveInvoiceAccountInfo > GetInvoiceAccountInfo( CancellationToken ct )
        {
            var currencies = await this.paginationManager.GetAll< Account >( SoqlQuery.Builder().Select( "Id", "Name" ).From( "Account" ) );
            return new SageLiveInvoiceAccountInfo
            {
                invoiceAccounts = currencies.ToDictionary( kv => kv.Id, kv => kv.Name )
            };
        }
    }
}