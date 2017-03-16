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
		protected string GetLogPrefix( SageLiveAuthInfo config, string addtionalInfo, [ CallerMemberName ] string methodName = "" )
		{
			return "{0} ({1}), credentials: {2}".FormatWith( methodName, addtionalInfo, config != null ? config._userId._userId : "Unknown" );
		}

		protected T ParseException< T >( string serviceName, bool isThrowException, Func< T > body )
		{
			try
			{
				return body();
			}
			catch( WebException ex )
			{
				var exNew = this.HandleException( ex, serviceName );
				if( isThrowException )
					throw exNew;
				else
					return default(T);
			}
		}

		protected async Task< T > ParseExceptionAsync< T >( string serviceName, bool isThrowException, Func< Task< T > > body )
		{
			try
			{
				return await body();
			}
			catch( WebException ex )
			{
				var exNew = this.HandleException( ex, serviceName );
				if( isThrowException )
					throw exNew;
				else
					return default(T);
			}
		}

		private WebException HandleException( WebException ex, string serviceName )
		{
			if( ex.Response == null || ex.Status != WebExceptionStatus.ProtocolError || ex.Response.ContentType == null )
			{
				SageLiveLogger.Error( ex, this.GetLogPrefix( null, serviceName ), "" );
				return ex;
			}

			var httpResponse = ( HttpWebResponse )ex.Response;

			using( var stream = httpResponse.GetResponseStream() )
			using( var reader = new StreamReader( stream ) )
			{
				var jsonResponse = reader.ReadToEnd();
				SageLiveLogger.Error( ex, this.GetLogPrefix( null, serviceName ), jsonResponse );
				return ex;
			}
		}
	}
}