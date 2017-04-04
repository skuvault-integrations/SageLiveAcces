using System;

namespace SageLiveAccess.Models.Auth
{
	class SageLiveFactoryConfig
	{
		public readonly String _clientId;
		public readonly String _clientSecret;
		public readonly String _redirectUri;

		public SageLiveFactoryConfig( String clientId, String clientSecret, String redirectUri )
		{
			this._clientId = clientId;
			this._clientSecret = clientSecret;
			this._redirectUri = redirectUri;
		}

		public SageLiveAuthRequest CreateAuthRequest( string code )
		{
			return new SageLiveAuthRequest
			{
				client_id = this._clientId,
				client_secret = this._clientSecret,
				redirect_uri = this._redirectUri,
				code = code
			};
		}
	}
}
