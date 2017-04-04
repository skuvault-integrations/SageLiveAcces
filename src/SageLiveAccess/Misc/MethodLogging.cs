using System;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Netco.Extensions;
using SageLiveAccess.Models.Auth;

namespace SageLiveAccess.Misc
{
	public class MethodLogging
	{
		protected string GetLogPrefix( SageLiveAuthInfo config, Mark mark, string addtionalInfo, [ CallerMemberName ] string methodName = "" )
		{
			return "Mark:{0}. {1} ({2}), credentials: {3}".FormatWith( mark, methodName, addtionalInfo, config != null ? config._userId._userId : "Unknown" );
		}

		protected T ParseException< T >( Mark mark, string serviceName, bool isThrowException, Func< T > body )
		{
			try
			{
				return body();
			}
			catch( WebException ex )
			{
				var exNew = this.HandleException( ex, mark, serviceName );
				if( isThrowException )
					throw exNew;
				else
					return default(T);
			}
		}

		protected async Task< T > ParseExceptionAsync< T >( Mark mark, string serviceName, bool isThrowException, Func< Task< T > > body )
		{
			try
			{
				return await body();
			}
			catch( WebException ex )
			{
				var exNew = this.HandleException( ex, mark, serviceName );
				if( isThrowException )
					throw exNew;
				else
					return default(T);
			}
		}

		private WebException HandleException( WebException ex, Mark mark, string serviceName )
		{
			if( ex.Response == null || ex.Status != WebExceptionStatus.ProtocolError || ex.Response.ContentType == null )
			{
				SageLiveLogger.Error( ex, this.GetLogPrefix( null, mark, serviceName ), "" );
				return ex;
			}

			var httpResponse = ( HttpWebResponse )ex.Response;

			using( var stream = httpResponse.GetResponseStream() )
			using( var reader = new StreamReader( stream ) )
			{
				var jsonResponse = reader.ReadToEnd();
				SageLiveLogger.Error( ex, this.GetLogPrefix( null, mark, serviceName ), jsonResponse );
				return ex;
			}
		}
	}
}