using System.Runtime.Serialization;

namespace SageLiveAccess.Models.Auth
{
	[ DataContract ]
	internal class SageLiveRefreshTokenResponse
	{
		[ DataMember( Name = "access_token" ) ]
		public string auth_token{ get; set; }
	}
}
