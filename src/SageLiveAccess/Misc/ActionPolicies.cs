using System;
using System.Threading.Tasks;
using Netco.ActionPolicyServices;

namespace SageLiveAccess.Misc
{
	public static class ActionPolicies
	{
#if DEBUG
		private const int RetryCount = 1;
#else
		private const int RetryCount = 5;
#endif
		private static readonly Func< int, Task > _delay = retryNum => Task.Delay( ( 1 + retryNum ) * 5000 );

		internal static ActionPolicyAsync GetAsyncQueryManagerActionPolicy( AsyncQueryManager manager, string prefix, string info, Mark mark )
		{
			return ActionPolicyAsync.Handle< Exception >().RetryAsync( RetryCount, async ( ex, i ) =>
			{
				SageLiveLogger.Debug( prefix, info +  string.Format( " Retrying request for {0} time, cause: {1} : {2}", i, ex.Message, ex.StackTrace ) );
				if( ex is SessionExpiredException )
				{
					await manager.RefreshTokenIfNeeded( mark );
				}
				else
					await _delay( i );
			} );
		}
	}
}
