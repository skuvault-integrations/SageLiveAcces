using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageLiveAccess.Models
{
	public class SaleInvoiceItem
	{        
		public Double Quantity { get; set; }
        public String ProductName { get; set; }
        public String ProductCode { get; set; }
        public String ProductUID { get; set; }
		public Double UnitPrice { get; set; }
	}
}
