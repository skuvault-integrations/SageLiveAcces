using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SageLiveAccess.Models.Auth;
using ServiceStack;
using ServiceStack.Text;

namespace SageLiveAccess
{
	class SageLiveAuthService: ISageLiveAuthService
	{
		private readonly HttpClient _client;
		private readonly SageLiveFactoryConfig _config;

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
			var request = ( HttpWebRequest )WebRequest.Create( "https://login.salesforce.com/services/oauth2/token" );
			var data = "grant_type=authorization_code&code={0}&client_id={1}&client_secret={2}&redirect_uri={3}".FormatWith( code, this._config._clientId, this._config._clientSecret, this._config._redirectUri );

			request.ContentType = "application/x-www-form-urlencoded";
			request.Method = WebRequestMethods.Http.Post;

			request.ContentLength = data.Length;

			using( StreamWriter stOut = new StreamWriter( request.GetRequestStream(), System.Text.Encoding.ASCII ) )
			{
				stOut.Write( data );
				stOut.Close();
			}

			return request;
		}

		private HttpWebRequest CreateGetUserRequest( SageLiveAuthResponse response )
		{
			var request = ( HttpWebRequest )WebRequest.Create( response.id );
			request.Method = WebRequestMethods.Http.Get;
			request.Headers.Add( HttpRequestHeader.Authorization, "Bearer {0}".FormatWith( response.access_token ) );
			return request;
		}

		public AuthResult AuthentifcateByCode( string code )
		{
			var getAuthTokenRequest = this.CreateSageLiveAuthRequest( code );
			var rawAuthResponse = getAuthTokenRequest.GetResponse();
			using( var authResponseStream = rawAuthResponse.GetResponseStream() )
			{
//				var x = new StreamReader( authResponseStream );
//				var s= x.ReadToEnd();

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
		}
	}
}