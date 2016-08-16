using System;
using SageLiveAccess.Models;
using SageLiveAccess.Models.Auth;

namespace SageLiveAccess
{
	public class SageLiveFactory : ISageLiveFactory
	{
		private readonly SageLiveFactoryConfig _config;

		public SageLiveFactory( string clientId, string secretId, string redirectUri )
		{
			this._config = new SageLiveFactoryConfig( clientId, secretId, redirectUri );
		}

		public ISageLiveSaleInvoiceSyncService CreateSageLiveInvoiceSyncService( SageLiveAuthInfo authInfo, SageLivePushInvoiceSettings settings, string currencyCode )
		{
			return new SageLiveSaleInvoiceSyncService( authInfo, this._config, settings, currencyCode );
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
