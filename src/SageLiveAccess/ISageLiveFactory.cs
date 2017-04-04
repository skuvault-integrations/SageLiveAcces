using SageLiveAccess.Models;
using SageLiveAccess.Models.Auth;

namespace SageLiveAccess
{
	public interface ISageLiveFactory
	{
		ISageLiveSaleInvoiceSyncService CreateSageLiveSaleInvoiceSyncService( SageLiveAuthInfo authInfo, SageLivePushInvoiceSettings settings, string currencyCode );
		ISageLivePurchaseInvoiceSyncService CreateSageLivePurchaseInvoiceSyncService( SageLiveAuthInfo authInfo, SageLivePushInvoiceSettings settings, string currencyCode );
		ISageLiveAuthService CreateSageLiveAuthService();
		ISageLiveSettingServicecs CreateSageLiveSettingsService( SageLiveAuthInfo authInfo );
	}
}