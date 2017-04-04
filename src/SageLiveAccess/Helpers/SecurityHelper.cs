using System.Net;

namespace SageLiveAccess.Helpers
{
	static class SecurityHelper
	{
		public static void SetSecurityProtocol()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls; // comparable to modern browsers
		}

		public static HttpWebRequest CreateWebRequest( string uri )
		{
			SetSecurityProtocol();
			return ( HttpWebRequest )WebRequest.Create( uri );
		}
	}
}