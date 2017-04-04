using System.Runtime.Serialization;

namespace SageLiveAccess.Models.Auth
{
	[ DataContract ]
	class SageLiveUserInfo
	{
		[ DataMember( Name = "user_id" ) ]
		public string user_id;

		[ DataMember( Name = "organization_id" ) ]
		public string organization_id;
	}
}
