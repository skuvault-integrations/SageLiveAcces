using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SageLiveAccess.Models.Auth;

namespace SageLiveAccess
{
	public interface ISageLiveAuthService
	{
		String GetAuthUrl();
		AuthResult AuthentifcateByCode( String code );
	}
}
