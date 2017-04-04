namespace SageLiveAccess.Models.Auth
{
	public class SageLiveRefreshToken
	{
		public readonly string _refreshToken;

		public SageLiveRefreshToken( string refreshToken )
		{
			this._refreshToken = refreshToken;
		}
	}
}
