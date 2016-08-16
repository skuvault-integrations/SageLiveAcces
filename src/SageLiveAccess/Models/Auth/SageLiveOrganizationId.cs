using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageLiveAccess.Models.Auth
{
	public class SageLiveOrganizationId
	{
		public readonly String _organizationId;

		public SageLiveOrganizationId( String organization )
		{
			this._organizationId = organization;
		}
	}
}
