using System;
using Netco.Logging;

namespace SageLiveAccess.Misc
{
	internal class SageLiveLogger
	{
		public static ILogger Log()
		{
			return NetcoLogger.GetLogger( "TrueShipLogger" );
		}

		public static void Debug( string prefix, string info )
		{
			Log().Debug( "[SageLive] {0}. {1}", prefix, info );
		}

		public static void Error( Exception ex, string prefix, string info )
		{
			Log().Error( ex, "[SageLive] {0}. {1}", prefix, info );
		}
	}
}