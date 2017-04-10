using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Netco.Extensions;
using SageLiveAccess.Misc;
using SageLiveAccess.Models;
using SageLiveAccess.Models.Auth;
using SageLiveAccess.Services;
using Task = System.Threading.Tasks.Task;

namespace SageLiveAccess
{
	class SageLiveSaleInvoiceSyncService: ISageLiveSaleInvoiceSyncService
	{
		private readonly PullInvoicesService pullInvoicesService;
		private readonly PushInvoiceService pushInvoicesService;
		private readonly string _currencyCode;

		public SageLiveSaleInvoiceSyncService( SageLiveAuthInfo authInfo, SageLiveFactoryConfig config, SageLivePushInvoiceSettings settings, string currencyCode )
		{
			var service = SaleForceConnectionCreator.CreateSforceService( authInfo );
			var asyncQueryManager = new AsyncQueryManager( service, config, authInfo._refreshToken._refreshToken );
			var paginationManager = new PaginationManager( asyncQueryManager );
			this.pullInvoicesService = new PullInvoicesService( asyncQueryManager, paginationManager );
			this.pushInvoicesService = new PushInvoiceService( asyncQueryManager, paginationManager, authInfo, settings );
			this._currencyCode = currencyCode;
		}

		public async Task< List< SaleInvoice > > GetSaleInvoices( DateTime dateFrom, DateTime dateTo, CancellationToken ct )
		{
			var mark = Mark.CreateNew();
			var inParams = "DateFrom:{0}, DateTo:{1}".FormatWith( dateFrom, dateTo );
			SageLiveLogger.LogStarted( mark, inParams );

			var result = await this.pullInvoicesService.GetSaleInvoices( dateFrom, dateTo, mark, ct );

			SageLiveLogger.LogEnd( mark, inParams, result?.MakeString() );
			return result;
		}

		public async Task PushSaleInvoices( IEnumerable< SaleInvoice > saleInvoices, CancellationToken ct )
		{
			var mark = Mark.CreateNew();
			SageLiveLogger.LogStarted( mark, saleInvoices?.MakeString() );

			await this.pushInvoicesService.PushSaleInvoices( saleInvoices, this._currencyCode, mark, ct );

			SageLiveLogger.LogEnd( mark, saleInvoices?.MakeString() );
		}
	}
}