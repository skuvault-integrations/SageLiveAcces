namespace SageLiveAccess.Models.Auth
{
	public class SageLiveAuthInfo
	{
		public readonly SageLiveSessionId _sessionId;
		public readonly SageLiveOrganizationId _organizationId;
		public readonly SageLiveUserId _userId;
		public readonly SageLiveInstanceUrl _instanceUrl;
		public readonly SageLiveRefreshToken _refreshToken;

		public SageLiveAuthInfo( SageLiveSessionId sessionId, SageLiveOrganizationId organizationId, SageLiveUserId userId, SageLiveInstanceUrl instanceUrl, SageLiveRefreshToken refreshToken )
		{
			this._sessionId = sessionId;
			this._organizationId = organizationId;
			this._userId = userId;
			this._instanceUrl = instanceUrl;
			this._refreshToken = refreshToken;
		}
	}
}
