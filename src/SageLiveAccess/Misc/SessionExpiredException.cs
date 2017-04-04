using System;

namespace SageLiveAccess.Misc
{
	internal class SessionExpiredException: Exception
	{
		public SessionExpiredException()
		{
		}

		public SessionExpiredException( string message )
			: base( message )
		{
		}

		public SessionExpiredException( string message, Exception inner )
			: base( message, inner )
		{
		}
	}
}
