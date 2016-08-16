using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SageLiveAccess.Misc;

namespace SageLiveAccess.Models
{
    public class SageLivePushInvoiceSettings
    {
        public readonly string _legislationId;
        public readonly string _companyName;

        public SageLivePushInvoiceSettings( string legislationId, string companyName )
        {
            this._companyName = companyName;
            this._legislationId = legislationId;
        }
    }
}
