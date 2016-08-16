using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using SageLiveAccess.Misc;
using SageLiveAccess.Models;
using SageLiveAccess.Models.Auth;
using SageLiveAccess.sforce;
using Task = System.Threading.Tasks.Task;
using Netco.Extensions;
using Netco.Monads;

namespace SageLiveAccess
{
	class SageLiveSaleInvoiceSyncService: ISageLiveSaleInvoiceSyncService
	{
		private readonly AsyncQueryManager asyncQueryManager;
		private readonly PaginationManager paginationManager;
		private readonly SageLiveAuthInfo _authInfo;
		private readonly PullInvoicesService pullInvoicesService;
		private readonly PushInvoiceService pushInvoicesService;
        private readonly string _currencyCode;

		public SageLiveSaleInvoiceSyncService( SageLiveAuthInfo authInfo, SageLiveFactoryConfig config, SageLivePushInvoiceSettings settings, string currencyCode )
		{
			var service = SaleForceConnectionCreator.CreateSforceService( authInfo );
			this.asyncQueryManager = new AsyncQueryManager( service, config, authInfo._refreshToken._refreshToken );
			this.paginationManager = new PaginationManager( this.asyncQueryManager );
			this._authInfo = authInfo;
			this.pullInvoicesService = new PullInvoicesService( this.asyncQueryManager, this.paginationManager, new SageLiveModelDescriber( service ) );
			this.pushInvoicesService = new PushInvoiceService( this.asyncQueryManager, this.paginationManager, authInfo, settings );
            this._currencyCode = currencyCode;
		}

		public async Task< List< SaleInvoice > > GetSaleInvoices( DateTime dateFrom, DateTime dateTo, CancellationToken ct )
		{
			return await this.pullInvoicesService.GetSaleInvoices( dateFrom, dateTo, ct );
		}

		public async Task PushSaleInvoices( IEnumerable< SaleInvoice > saleInvoices, CancellationToken ct )
		{
			await this.pushInvoicesService.PushSaleInvoices( saleInvoices, this._currencyCode, ct );
		}
	}
}