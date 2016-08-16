using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Netco.Extensions;
using SageLiveAccess.Models.Auth;

namespace SageLiveAccess.Misc
{
	public class MethodLogging
	{
		protected string GetLogPrefix( SageLiveAuthInfo config, string addtionalInfo, [CallerMemberName] string methodName = "" )
		{
			return "{0} ({1}), credentials: {2}".FormatWith( methodName, addtionalInfo, config != null ? config._userId._userId : "Unknown" );
		}
	}
}
