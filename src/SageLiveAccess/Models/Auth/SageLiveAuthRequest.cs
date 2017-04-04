using System;
using System.Runtime.Serialization;

namespace SageLiveAccess.Models.Auth
{
	[ DataContract ]
	class SageLiveAuthRequest
	{
		[ DataMember( Name = "grant_type" ) ]
		public String grant_type{ get; set; }

		[ DataMember( Name = "code" ) ]
		public String code{ get; set; }

		[ DataMember( Name = "client_id" ) ]
		public String client_id{ get; set; }

		[ DataMember( Name = "client_secret" ) ]
		public String client_secret{ get; set; }

		[ DataMember( Name = "redirect_uri" ) ]
		public String redirect_uri{ get; set; }
	}
}