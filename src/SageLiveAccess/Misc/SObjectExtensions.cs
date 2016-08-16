using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SageLiveAccess.Models;
using SageLiveAccess.sforce;

namespace SageLiveAccess.Misc
{
	public static class SObjectExtensions
	{

        public static string MakeString( this sObject[] objects )
        {
            return string.Join( ",", objects.Select( o => o.Id ) );
        }

        public static string MakeString( this string[] objects )
        {
            return string.Join( ", ", objects );
        }


        public static string MakeString( this List< sObject > objects )
		{
			return string.Join( ",", objects.Select( o => o.Id) );
		}

		public static string MakeString( this List< s2cor__Sage_INV_Trade_Document_Item__c > objects )
		{
			return string.Join( ",", objects.Select( o => o.Id ) );
		}

		public static string MakeString( this IEnumerable< SaleInvoice > objects )
		{
			return string.Join( ",", objects.Select( o => o.UID ) );
		}

		public static string MakeString( this IEnumerable< SaleInvoiceItem > objects )
		{
			return string.Join( ",", objects.Select( o => o.ProductCode ) );
		}
	}
}
