using SageLiveAccess.Models;
using SageLiveAccess.Models.Auth;

namespace SageLiveAccess
{
	public class SageLiveFactory: ISageLiveFactory
	{
		private readonly SageLiveFactoryConfig _config;

		public SageLiveFactory( string clientId, string secretId, string redirectUri )
		{
			this._config = new SageLiveFactoryConfig( clientId, secretId, redirectUri );
		}

		public ISageLiveSaleInvoiceSyncService CreateSageLiveSaleInvoiceSyncService( SageLiveAuthInfo authInfo, SageLivePushInvoiceSettings settings, string currencyCode )
		{
			return new SageLiveSaleInvoiceSyncService( authInfo, this._config, settings, currencyCode );
		}

		public ISageLivePurchaseInvoiceSyncService CreateSageLivePurchaseInvoiceSyncService( SageLiveAuthInfo authInfo, SageLivePushInvoiceSettings settings, string currencyCode )
		{
			return new SageLivePurchaseInvoiceSyncService( authInfo, this._config, settings, currencyCode );
		}

		public ISageLiveAuthService CreateSageLiveAuthService()
		{
			return new SageLiveAuthService( this._config );
		}

		public ISageLiveSettingServicecs CreateSageLiveSettingsService( SageLiveAuthInfo authInfo )
		{
			return new SageLiveSettingsService( authInfo, this._config );
		}
	}
}
