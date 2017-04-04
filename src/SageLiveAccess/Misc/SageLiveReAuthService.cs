using System.IO;
using System.Net;
using System.Threading.Tasks;
using Netco.Extensions;
using SageLiveAccess.Helpers;
using SageLiveAccess.Models.Auth;
using ServiceStack.Text;

namespace SageLiveAccess.Misc
{
	internal class SageLiveReAuthService: MethodLogging
	{
		private readonly SageLiveFactoryConfig _config;

		public SageLiveReAuthService( SageLiveFactoryConfig config )
		{
			this._config = config;
		}

		private HttpWebRequest CreateSageLiveReAuthRequest( string refreshToken )
		{
			var request = SecurityHelper.CreateWebRequest( "https://login.salesforce.com/services/oauth2/token" );
			var data = "grant_type=refresh_token&refresh_token={0}&client_id={1}&client_secret={2}".FormatWith( refreshToken, this._config._clientId, this._config._clientSecret );

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

		public async Task< string > GetRefreshedToken( string refreshToken, Mark mark )
		{
			return await this.ParseExceptionAsync( mark, "SageLiveReAuthService", true, async () =>
			{
				var request = this.CreateSageLiveReAuthRequest( refreshToken );
				var response = await request.GetResponseAsync();
				using( var refreshTokenReponseStream = response.GetResponseStream() )
				{
					var refreshTokenResponse = JsonSerializer.DeserializeFromStream< SageLiveRefreshTokenResponse >( refreshTokenReponseStream );
					return refreshTokenResponse.auth_token;
				}
			} );
		}
	}
}