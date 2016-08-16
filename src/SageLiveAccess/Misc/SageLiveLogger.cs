using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netco.Logging;

namespace SageLiveAccess.Misc
{
	class SageLiveLogger
	{
		public static ILogger Log()
		{
			return NetcoLogger.GetLogger( "TrueShipLogger" );
		}

		public static void Debug( string prefix, string info )
		{
			Log().Debug( "[SageLive] {0}. {1}", prefix, info );
		}
	}
}
