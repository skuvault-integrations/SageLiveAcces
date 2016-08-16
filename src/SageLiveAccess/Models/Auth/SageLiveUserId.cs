using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageLiveAccess.Models.Auth
{
	public class SageLiveUserId
	{
		public readonly string _userId;

		public SageLiveUserId( string userId )
		{
			this._userId = userId;
		}
	}
}
