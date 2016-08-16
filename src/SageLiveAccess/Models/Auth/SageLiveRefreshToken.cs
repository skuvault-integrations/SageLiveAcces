using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageLiveAccess.Models.Auth
{
	public class SageLiveRefreshToken
	{
		public readonly string _refreshToken;

		public SageLiveRefreshToken( string refreshToken )
		{
			this._refreshToken = refreshToken;
		}
	}
}
