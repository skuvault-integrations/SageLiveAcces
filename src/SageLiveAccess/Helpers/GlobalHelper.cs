using System.Net;

namespace SageLiveAccess.Helpers
{
	static class GlobalHelper
	{
		/// <summary>
		/// [dirty hack] Created as temp solution and added everywhere because this project does not have one point for all calls. :(
		/// </summary>
		public static void SetSecurityProtocol()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls; // comparable to modern browsers
		}
	}
}