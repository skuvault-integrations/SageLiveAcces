﻿using System.Collections.Generic;
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
