using System;

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
