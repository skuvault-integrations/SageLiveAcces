using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SageLiveAccess.Misc;
using SageLiveAccess.Models;
using SageLiveAccess.Models.Auth;

namespace SageLiveAccess
{
	class SageLivePurchaseInvoiceSyncService : ISageLivePurchaseInvoiceSyncService
	{

		private readonly AsyncQueryManager asyncQueryManager;
		private readonly PaginationManager paginationManager;
		private readonly PushInvoiceService pushInvoicesService;
		private readonly string _currencyCode;

		public SageLivePurchaseInvoiceSyncService( SageLiveAuthInfo authInfo, SageLiveFactoryConfig config, SageLivePushInvoiceSettings settings, string currencyCode )
		{
			var service = SaleForceConnectionCreator.CreateSforceService( authInfo );
			this.asyncQueryManager = new AsyncQueryManager( service, config, authInfo._refreshToken._refreshToken );
			this.paginationManager = new PaginationManager( this.asyncQueryManager );
			this.pushInvoicesService = new PushInvoiceService( this.asyncQueryManager, this.paginationManager, authInfo, settings );
			this._currencyCode = currencyCode;
		}

		public async Task PushPurchaseInvoices( IEnumerable< PurchaseInvoice > saleInvoices, CancellationToken ct )
		{
			await this.pushInvoicesService.PushPurchaseInvoices( saleInvoices, this._currencyCode, ct );
		}
	}
}
