using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SageLiveAccess.Models;

namespace SageLiveAccess
{
	public interface ISageLivePurchaseInvoiceSyncService
	{
		Task PushPurchaseInvoices( IEnumerable< PurchaseInvoice > saleInvoices, CancellationToken ct );
	}
}
