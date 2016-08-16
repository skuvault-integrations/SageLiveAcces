using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SageLiveAccess.Models.Auth
{
	[ DataContract ]
	internal class SageLiveRefreshTokenResponse
	{
		[ DataMember( Name = "access_token" ) ]
		public string auth_token{ get; set; }
	}
}
