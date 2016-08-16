using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using SageLiveAccess.Models.Auth;
using SageLiveAccess.sforce;
using ServiceStack;

namespace SageLiveAccess.Misc
{
	internal static class SaleForceConnectionCreator
	{
		private static String GetEndpointUrl( SageLiveAuthInfo authInfo )
		{
			return Conventions.EndPointFormat.FormatWith( authInfo._instanceUrl._instanceUrl, Conventions.SoapApiVersion, authInfo._organizationId, authInfo._userId );
		}

		public static SforceService CreateSforceService( SageLiveAuthInfo authInfo )
		{
			var binding = new SforceService();

			binding.Url = GetEndpointUrl( authInfo );
			binding.SessionHeaderValue = new SessionHeader();
			binding.SessionHeaderValue.sessionId = authInfo._sessionId._sessionId;
			return binding;
		}
	}
}