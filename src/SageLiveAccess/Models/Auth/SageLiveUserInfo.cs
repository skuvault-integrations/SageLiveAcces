using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
