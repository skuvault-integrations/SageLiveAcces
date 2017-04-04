using System;

namespace SageLiveAccess.Models
{
	public class InvoiceItem
	{        
		public Double Quantity { get; set; }
        public String ProductName { get; set; }
        public String ProductCode { get; set; }
        public String ProductUID { get; set; }
		public Double UnitPrice { get; set; }
	}
}
