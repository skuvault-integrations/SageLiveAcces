using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SageLiveAccess.Misc;
using SageLiveAccess.Models;
using SageLiveAccess.Models.Auth;
using SageLiveAccess.Services;

namespace SageLiveAccess
{
	class SageLivePurchaseInvoiceSyncService: ISageLivePurchaseInvoiceSyncService
	{
		private readonly PushInvoiceService pushInvoicesService;
		private readonly string _currencyCode;

		public SageLivePurchaseInvoiceSyncService( SageLiveAuthInfo authInfo, SageLiveFactoryConfig config, SageLivePushInvoiceSettings settings, string currencyCode )
		{
			var service = SaleForceConnectionCreator.CreateSforceService( authInfo );
			var asyncQueryManager = new AsyncQueryManager( service, config, authInfo._refreshToken._refreshToken );
			var paginationManager = new PaginationManager( asyncQueryManager );
			this.pushInvoicesService = new PushInvoiceService( asyncQueryManager, paginationManager, authInfo, settings );
			this._currencyCode = currencyCode;
		}

		public async Task PushPurchaseInvoices( IEnumerable< PurchaseInvoice > saleInvoices, CancellationToken ct )
		{
			var mark = Mark.CreateNew();
			SageLiveLogger.LogStarted( mark, saleInvoices?.MakeString() );

			await this.pushInvoicesService.PushPurchaseInvoices( saleInvoices, this._currencyCode, mark, ct );

			SageLiveLogger.LogEnd( mark, saleInvoices?.MakeString(), String.Empty );
		}
	}
}
