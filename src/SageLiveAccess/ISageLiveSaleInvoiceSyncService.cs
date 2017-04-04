using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SageLiveAccess.Models;

namespace SageLiveAccess
{
	public interface ISageLiveSaleInvoiceSyncService
	{
		Task< List< SaleInvoice > > GetSaleInvoices( DateTime dateFrom, DateTime dateTo, CancellationToken ct );
		Task PushSaleInvoices( IEnumerable< SaleInvoice > saleInvoices, CancellationToken ct );
	}
}
