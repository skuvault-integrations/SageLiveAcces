using System;
using SageLiveAccess.Models.Auth;

namespace SageLiveAccess
{
	public interface ISageLiveAuthService
	{
		String GetAuthUrl();
		AuthResult AuthentifcateByCode( String code );
	}
}
