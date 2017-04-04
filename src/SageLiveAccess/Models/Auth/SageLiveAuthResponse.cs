using System.Runtime.Serialization;

namespace SageLiveAccess.Models.Auth
{
	[ DataContract ]
	class SageLiveAuthResponse
	{
		[ DataMember( Name = "id" ) ]
		public string id{ get; set; }

		[ DataMember( Name = "refresh_token" ) ]
		public string refresh_token{ get; set; }

		[ DataMember( Name = "issued_at" ) ]
		public string issued_at{ get; set; }

		[ DataMember( Name = "instance_url" ) ]
		public string instance_url{ get; set; }

		[ DataMember( Name = "signature" ) ]
		public string signature{ get; set; }

		[ DataMember( Name = "access_token" ) ]
		public string access_token{ get; set; }
	}
}