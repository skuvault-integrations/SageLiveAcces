using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SageLiveAccess.Models;
using SageLiveAccess.Models.Auth;

namespace SageLiveAccess
{
	public interface ISageLiveFactory
	{

        ISageLiveSaleInvoiceSyncService CreateSageLiveInvoiceSyncService( SageLiveAuthInfo authInfo, SageLivePushInvoiceSettings settings, string currencyCode );
        ISageLiveAuthService CreateSageLiveAuthService();
        ISageLiveSettingServicecs CreateSageLiveSettingsService( SageLiveAuthInfo authInfo );
	}
}