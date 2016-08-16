using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SageLiveAccess.Models;

namespace SageLiveAccess
{
	public interface ISageLiveSettingServicecs
	{
		Task< SageLiveLegislationInfo > GetLegislationInfo( CancellationToken ct );
		Task< SageLiveInvoiceAccountInfo > GetInvoiceAccountInfo( CancellationToken ct );
	}
}
