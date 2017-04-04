using System.IO;
using System.Net;
using System.Text;
using SageLiveAccess.Helpers;
using SageLiveAccess.Misc;
using SageLiveAccess.Models.Auth;
using ServiceStack;
using ServiceStack.Text;

namespace SageLiveAccess
{
	internal class SageLiveAuthService: MethodLogging, ISageLiveAuthService
	{
		private readonly SageLiveFactoryConfig _config;
		private const string ServiceName = "SageLiveAuthService";

		public SageLiveAuthService( SageLiveFactoryConfig config )
		{
			this._config = config;
		}

		public string GetAuthUrl()
		{
			return string.Format( "https://login.salesforce.com/services/oauth2/authorize?response_type=code&client_id={0}&redirect_uri={1}", this._config._clientId, this._config._redirectUri );
		}

		private HttpWebRequest CreateSageLiveAuthRequest( string code )
		{
			var request = SecurityHelper.CreateWebRequest( "https://login.salesforce.com/services/oauth2/token" );
			var data = "grant_type=authorization_code&code={0}&client_id={1}&client_secret={2}&redirect_uri={3}".FormatWith( code, this._config._clientId, this._config._clientSecret, this._config._redirectUri );

			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = WebRequestMethods.Http.Post;

			request.ContentLength = data.Length;

			using( var stOut = new StreamWriter( request.GetRequestStream(), Encoding.ASCII ) )
			{
				stOut.Write( data );
				stOut.Close();
			}

			return request;
		}

		private HttpWebRequest CreateGetUserRequest( SageLiveAuthResponse response )
		{
			var request = SecurityHelper.CreateWebRequest( response.id );
			request.Method = WebRequestMethods.Http.Get;
			request.Headers.Add( HttpRequestHeader.Authorization, "Bearer {0}".FormatWith( response.access_token ) );
			return request;
		}

		public AuthResult AuthentifcateByCode( string code )
		{
			var mark = Mark.CreateNew();
			SageLiveLogger.LogStarted(mark, code);
			var result =  this.ParseException( mark, ServiceName, true, () =>
			{
				var getAuthTokenRequest = this.CreateSageLiveAuthRequest( code );
				var rawAuthResponse = getAuthTokenRequest.GetResponse();
				using( var authResponseStream = rawAuthResponse.GetResponseStream() )
				{
					var authResponse = JsonSerializer.DeserializeFromStream< SageLiveAuthResponse >( authResponseStream );

					var getUserRequest = this.CreateGetUserRequest( authResponse );
					var rawGetUserResponse = getUserRequest.GetResponse();

					using( var userInfoReponseStream = rawGetUserResponse.GetResponseStream() )
					{
						var getUserResponse = JsonSerializer.DeserializeFromStream< SageLiveUserInfo >( userInfoReponseStream );

						return new AuthResult
						{
							sessionId = authResponse.access_token,
							organizationId = getUserResponse.organization_id,
							userId = getUserResponse.user_id,
							instanceUrl = authResponse.instance_url,
							refreshToken = authResponse.refresh_token
						};
					}
				}
			} );

			SageLiveLogger.LogEnd( mark, code, result != null ? result.sessionId : string.Empty );
			return result;
		}
	}
}