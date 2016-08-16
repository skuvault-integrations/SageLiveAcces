using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageLiveAccess.Models.Auth
{
	public class AuthResult
	{
		public String sessionId;
		public String userId;
		public String organizationId;
		public String instanceUrl;
		public String refreshToken;
	}
}
