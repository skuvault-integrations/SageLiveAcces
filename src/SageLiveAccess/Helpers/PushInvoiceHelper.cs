using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Netco.Extensions;
using Netco.Monads;
using SageLiveAccess.Misc;
using SageLiveAccess.Models;
using SageLiveAccess.Models.Auth;
using SageLiveAccess.sforce;

namespace SageLiveAccess.Helpers
{
	internal class PresentAndAbsentInvoiceInfo
	{
		public readonly List< InvoiceBase > _invoicesToCreate;
		public readonly List< KeyValuePair< InvoiceBase, string > > _invoicesToUpdate;

		public PresentAndAbsentInvoiceInfo()
		{
			this._invoicesToUpdate = new List< KeyValuePair< InvoiceBase, string > >();
			this._invoicesToCreate = new List< InvoiceBase >();
		}
	}

	internal class PushInvoiceHelper : MethodLogging
	{
		private readonly AsyncQueryManager _asyncQueryManager;
		private readonly PaginationManager _paginationManager;
		private readonly SageLiveAuthInfo _authInfo;
        private readonly SageLivePushInvoiceSettings _pushSettings;
        private readonly CompanyHelper _companyHelper;

		private const string ServiceName = "PushInvoiceHelper";

		public PushInvoiceHelper( AsyncQueryManager asyncQueryManager, PaginationManager paginationManager, SageLiveAuthInfo authInfo, SageLivePushInvoiceSettings pushSettings )
		{
			this._asyncQueryManager = asyncQueryManager;
			this._paginationManager = paginationManager;
			this._authInfo = authInfo;
            this._companyHelper = new CompanyHelper( this._asyncQueryManager );
            this._pushSettings = pushSettings;
		}

//		private async Task< string > GetAccountId( SaleInvoice saleInvoice )
//		{
//			var warnMsg = "Using default account Id for sale invoice #{0}".FormatWith( saleInvoice.UID );
//
//			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, ServiceName ), "Getting account Id for sale invoice #{0}".FormatWith( saleInvoice.UID ) );
//			if ( saleInvoice.ContactInfo == null || saleInvoice.ContactInfo.LastName == null )
//			{
//				SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, ServiceName ), warnMsg );
//                return this._pushSettings._invoiceAccountId;
//			}
//			var contact = await this._asyncQueryManager.QueryOneAsync< Contact >( "SELECT Id, AccountId FROM Contact WHERE Name LIKE '{0}'".FormatWith( saleInvoice.ContactInfo.LastName ) );
//			if( !contact.HasValue )
//			{
//				SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, ServiceName ), warnMsg );
//				return this._pushSettings._invoiceAccountId;
//            }
//				
//			return contact.Value.AccountId;
//		}

//		private async Task< Account > GetDefaultAccountId()
//		{
//			return ( await this._asyncQueryManager.QueryOneAsync< Account >( "SELECT Id FROM Account" ) ).Value;
//		}
//
//		private async Task< s2cor__Sage_COR_Company__c > GetDefaultCompany()
//		{
//			return ( await this._asyncQueryManager.QueryOneAsync< s2cor__Sage_COR_Company__c >( "SELECT Id, Name, s2cor__Legislation__c, s2cor__Base_Currency__c FROM s2cor__Sage_COR_Company__c" ) ).Value;
//		}

		private async Task< s2cor__Sage_COR_Company__c > GetCompanyId( InvoiceBase invoiceBase, string currencyId )
		{
			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, ServiceName ), "Getting company Id for invoice #{0}".FormatWith( invoiceBase.UID ) );

			var companyInfo = new SageLiveCompanyModel
		    {
		        BaseCurrency = currencyId,
		        LegislationId = this._pushSettings._legislationId,
		        Name = ( invoiceBase is SaleInvoice ) ? this._pushSettings._companyName : invoiceBase.ContactInfo.Company
		    };	
			return ( await this._asyncQueryManager.QueryOneAsync< s2cor__Sage_COR_Company__c >( SoqlQuery.Builder().Select( "Id", "Name", "s2cor__Legislation__c", "s2cor__Base_Currency__c" ).From( "s2cor__Sage_COR_Company__c" ).Where( "Name" ).IsEqualTo( this._pushSettings._companyName ) ) ).GetValue( await this._companyHelper.GetOrCreateCompany( companyInfo ) );
		}

		public async Task< Maybe< s2cor__Sage_INV_Trade_Document__c > > GetInvoice( InvoiceBase invoice )
		{
			return ( await this._asyncQueryManager.QueryOneAsync< s2cor__Sage_INV_Trade_Document__c >( SoqlQuery.Builder().Select( "Id" ).From( "s2cor__Sage_INV_Trade_Document__c" ).Where( "s2cor__UID__c" ).IsEqualTo( invoice.UID ) ) );
		}

		public async Task< PresentAndAbsentInvoiceInfo > GetPresentAndAbsentInvoiceInfo( IEnumerable< InvoiceBase > saleInvoices )
		{
			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, ServiceName ), "Getting present and absent invoices for push selection..." );
			var result = new PresentAndAbsentInvoiceInfo();
			foreach( var invoice in saleInvoices )
			{
				var dbEntry = await this.GetInvoice( invoice );
				if( !dbEntry.HasValue )
					result._invoicesToCreate.Add( invoice );
				else
					result._invoicesToUpdate.Add( new KeyValuePair< InvoiceBase, string >( invoice, dbEntry.Value.Id ) );
			}
			return result;
		}

		public async Task< sObject > CreateInvoice( InvoiceBase invoiceModel, string invoiceTypeId, string currencyId, string dimensionId )
		{
			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, ServiceName ), "Creating invoice sObject for invoice #{0}".FormatWith( invoiceModel.UID ) );
			var company = await this.GetCompanyId( invoiceModel, currencyId );
			var invoice = new s2cor__Sage_INV_Trade_Document__c();
			invoice.s2cor__Description__c = invoiceModel.Description ?? "";
			invoice.s2cor__UID__c = invoiceModel.UID;
			invoice.s2cor__Company__c = company.Id;
			invoice.s2cor__Currency__c = company.s2cor__Base_Currency__c; 
			invoice.s2cor__Status__c = "Submitted";
			invoice.s2cor__Paid_Amount__c = invoiceModel.Total;
			invoice.s2cor__Date__c = invoiceModel.CreationDate; 
			invoice.OwnerId = this._authInfo._userId._userId;
			invoice.s2cor__Account__c = ( await this._companyHelper.GetOrCreateAccount( invoiceModel ) ).Id;
			invoice.s2cor__Approval_Status__c = "Approved";

			var documentNumber = new s2cor__Sage_ACC_Tag__c();
			documentNumber.s2cor__Company__c = company.Id;
			documentNumber.s2cor__Base_Credit__c = ( invoiceModel is SaleInvoice ) ? invoiceModel.Total : 0;
			documentNumber.s2cor__Base_Credit__cSpecified = true;
			documentNumber.s2cor__Dimension__c = dimensionId;
			documentNumber.Name = invoiceModel.UID;

			var documentNumberId = await this._asyncQueryManager.Insert( new[] { documentNumber } );
			invoice.s2cor__Document_Number_Tag__c = documentNumberId.First().id;


			invoice.s2cor__Legislation__c = company.s2cor__Legislation__c; 
			invoice.s2cor__Trade_Document_Type__c = invoiceTypeId; 
			var contactMaybe = await this._companyHelper.GetOrCreateContact( invoiceModel, invoice.s2cor__Account__c );

			if ( invoiceModel is SaleInvoice && contactMaybe.HasValue )
			{
				invoice.s2cor__Contact__c = contactMaybe.Value.Id;
			}
			
			return invoice;
		}

		public async Task< List< sObject > > CreateSaleInvoices( IEnumerable< InvoiceBase > saleInvoices, string salesInvoiceDocumentTypeId, string currencyId, string dimensionId )
		{
			var invoices = new List< sObject >();
			foreach ( var saleInvoice in saleInvoices )
			{
				invoices.Add( await this.CreateInvoice( saleInvoice, salesInvoiceDocumentTypeId, currencyId, dimensionId ) );
			}

			return invoices;
		}
	}
}