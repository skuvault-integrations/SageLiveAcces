using System;
using System.Collections.Generic;
using SageLiveAccess.sforce;

namespace SageLiveAccess.Models
{
	public class InvoiceBase : SageLiveModel
	{
		public string UID { get; set; }
        public string AccountName { get; set; }
		public string Description { get; set; }
		public string InvoiceNumber { get; set; }
		public DateTime CreationDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public String Status { get; set; }
        public List< InvoiceItem > Items { get; set; }
		public ContactInfo ContactInfo{ get; set; }
		public AddressInfo AddressInfo{ get; set; }
        public String FulfilledBy { get; set; }
        public double Total { get; set; }
        public bool IsPaid { get; set; }
	}
	
}
