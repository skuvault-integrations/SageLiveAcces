using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SageLiveAccess.Helpers;
using SageLiveAccess.Misc;
using SageLiveAccess.Models;
using SageLiveAccess.sforce;
using Task = System.Threading.Tasks.Task;

namespace SageLiveAccess.Services
{
	internal class PullInvoicesService
	{
		private readonly AsyncQueryManager _asyncQueryManager;
		private readonly PaginationManager _paginationManager;
		private readonly DocumentTypeHelper _documentTypeHelper;

		public PullInvoicesService( AsyncQueryManager asyncQueryManager, PaginationManager paginationManager )
		{
			this._asyncQueryManager = asyncQueryManager;
			this._paginationManager = paginationManager;
			this._documentTypeHelper = new DocumentTypeHelper( this._asyncQueryManager );
		}

		private string FormatDate( DateTime dateTime )
		{
			return dateTime.ToString( "yyyy-MM-ddThh:mm:ssZ" );
		}

		public async Task< List< SaleInvoice > > GetSaleInvoices( DateTime dateFrom, DateTime dateTo, Mark mark, CancellationToken ct )
		{
			var invoiceType = await this._documentTypeHelper.GetSaleInvoiceTypeId( mark, ct );
			if( !invoiceType.HasValue )
				return new List< SaleInvoice >();

			var invoiceTypeId = invoiceType.Value.Id;

			// get raw invoices
			var rawInvoices = await this._paginationManager.GetAll< s2cor__Sage_INV_Trade_Document__c >( SoqlQuery.Builder().Select( "Id", "Name", "s2cor__UID__c", "s2cor__Company__c", "s2cor__Date__c", "s2cor__Currency__c", "s2cor__Status__c", "SkuVault_Sage__Fulfilled_By__c", "s2cor__Is_Paid__c", "s2cor__Contact__c", "s2cor__Total_Amount__c", "LastModifiedDate", "s2cor__Document_Number_Tag__c", "s2cor__Document_Number__c" ).From( "s2cor__Sage_INV_Trade_Document__c" ).Where( "LastModifiedDate" ).IsGreaterThan( dateFrom ).And( "LastModifiedDate" ).IsLessThan( dateTo ).And( "s2cor__Trade_Document_Type__c" ).IsEqualTo( invoiceTypeId ), mark, ct /*"SELECT Id, Name, s2cor__UID__c, s2cor__Company__c, s2cor__Date__c, s2cor__Currency__c, s2cor__Status__c, SkuVault_Sage__Fulfilled_By__c, s2cor__Is_Paid__c, s2cor__Contact__c, s2cor__Total_Amount__c, LastModifiedDate FROM s2cor__Sage_INV_Trade_Document__c WHERE LastModifiedDate > {0} AND LastModifiedDate < {1} AND s2cor__Trade_Document_Type__c = '{2}'".FormatWith( this.FormatDate( dateFrom ), this.FormatDate( dateTo ), invoiceTypeId )*/ );

			var invoices = rawInvoices.Select( async rawInvoice =>
			{
				// get invoice items
				var items = await this.GetSaleInvoiceItems( rawInvoice, mark, ct );

				// get contact and address info
				var contactAndAddressInfo = await this.GetContactAndAddressInfo( rawInvoice, mark, ct );

				var invoice = new SaleInvoice
				{
					UID = rawInvoice.s2cor__UID__c,
					InvoiceNumber = rawInvoice.Name,
					CreationDate = rawInvoice.s2cor__Date__c ?? DateTime.UtcNow,
					FulfilledBy = rawInvoice.SkuVault_Sage__Fulfilled_By__c,
					LastModifiedDate = rawInvoice.LastModifiedDate ?? DateTime.UtcNow,
					Status = rawInvoice.s2cor__Status__c,
					Items = items,
					ContactInfo = contactAndAddressInfo.ContactInfo,
					AddressInfo = contactAndAddressInfo.AddressInfo,
					IsPaid = rawInvoice.s2cor__Is_Paid__c ?? false,
					Total = rawInvoice.s2cor__Total_Amount__c ?? 0
				};
				return invoice;
			} );

			return ( await Task.WhenAll( invoices ) ).ToList();
		}

		public async Task< ContactAndAddressInfo > GetContactAndAddressInfo( s2cor__Sage_INV_Trade_Document__c saleInvoice, Mark mark, CancellationToken ct )
		{
			var company = ( await this._asyncQueryManager.QueryOneAsync< s2cor__Sage_COR_Company__c >( SoqlQuery.Builder().Select( "Name" ).From( "s2cor__Sage_COR_Company__c" ).Where( "Id" ).IsEqualTo( saleInvoice.s2cor__Company__c ), mark, ct ) ).GetValue( () => new s2cor__Sage_COR_Company__c() );
			var contact = ( await this._asyncQueryManager.QueryOneAsync< Contact >( SoqlQuery.Builder().Select( "Name", "MailingAddress", "FirstName", "LastName", "MailingCity", "MailingState", "MailingStreet", "MailingPostalCode" ).From( "Contact" ).Where( "Id" ).IsEqualTo( saleInvoice.s2cor__Contact__c ), mark, ct ) ).GetValue( () => new Contact() );

			var contactInfo = new ContactInfo
			{
				FirstName = contact.FirstName ?? "N/A",
				LastName = contact.LastName ?? "N/A",
				Company = company.Name ?? "N/A"
			};
			var addressInfo = new AddressInfo
			{
				City = contact.MailingCity ?? "N/A",
				State = contact.MailingState ?? "N/A",
				Street = contact.MailingStreet ?? "N/A",
				Zip = contact.MailingPostalCode ?? "N/A"
			};

			return new ContactAndAddressInfo
			{
				AddressInfo = addressInfo,
				ContactInfo = contactInfo
			};
		}

		public async Task< List< InvoiceItem > > GetSaleInvoiceItems( s2cor__Sage_INV_Trade_Document__c saleInvoice, Mark mark, CancellationToken ct )
		{
			// get raw items
			var rawItems = await this._paginationManager.GetAll< s2cor__Sage_INV_Trade_Document_Item__c >( SoqlQuery.Builder().Select( "s2cor__Trade_Document__c", "s2cor__Quantity__c", "s2cor__Product__c", "s2cor__Unit_Price__c" ).From( "s2cor__Sage_INV_Trade_Document_Item__c" ).Where( "s2cor__Trade_Document__c" ).IsEqualTo( saleInvoice.Id ), mark, ct /*string.Format( "SELECT s2cor__Trade_Document__c, s2cor__Quantity__c, s2cor__Product__c, s2cor__Unit_Price__c FROM s2cor__Sage_INV_Trade_Document_Item__c WHERE s2cor__Trade_Document__c = '{0}'", saleInvoice.Id ) */ );
			var items = rawItems.Select( async rawItem =>
			{
				var item = new InvoiceItem();

				var rawProduct = await this._asyncQueryManager.QueryOneAsync< Product2 >( SoqlQuery.Builder().Select( "Name", "Description", "s2cor__UID__c", "ProductCode" ).From( "Product2" ).Where( "Id" ).IsEqualTo( rawItem.s2cor__Product__c ), mark, ct );

				if( !rawProduct.HasValue )
					return item;

				item.ProductCode = rawProduct.Value.ProductCode;
				item.ProductName = rawProduct.Value.Name;
				item.UnitPrice = rawItem.s2cor__Unit_Price__c ?? 0;
				item.ProductUID = rawProduct.Value.s2cor__UID__c;
				item.Quantity = rawItem.s2cor__Quantity__c ?? 0;

				return item;
			} );
			return ( await Task.WhenAll( items ) ).ToList();
		}
	}
}
