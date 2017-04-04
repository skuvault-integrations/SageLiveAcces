using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
		//public readonly List< KeyValuePair< InvoiceBase, string > > _invoicesToUpdate;

		public PresentAndAbsentInvoiceInfo()
		{
			//this._invoicesToUpdate = new List< KeyValuePair< InvoiceBase, string > >();
			this._invoicesToCreate = new List< InvoiceBase >();
		}
	}

	internal class PushInvoiceHelper: MethodLogging
	{
		private readonly AsyncQueryManager _asyncQueryManager;
		private readonly SageLiveAuthInfo _authInfo;
		private readonly SageLivePushInvoiceSettings _pushSettings;
		private readonly CompanyHelper _companyHelper;

		private const string ServiceName = "PushInvoiceHelper";

		public PushInvoiceHelper( AsyncQueryManager asyncQueryManager, SageLiveAuthInfo authInfo, SageLivePushInvoiceSettings pushSettings )
		{
			this._asyncQueryManager = asyncQueryManager;
			this._authInfo = authInfo;
			this._companyHelper = new CompanyHelper( this._asyncQueryManager );
			this._pushSettings = pushSettings;
		}

		private bool AreRegionSettingsChanged( s2cor__Sage_COR_Company__c company, string currencyId )
		{
			return ( !company.s2cor__Legislation__c.Equals( this._pushSettings._legislationId ) || !company.s2cor__Base_Currency__c.Equals( currencyId ) );
		}

		private async Task< s2cor__Sage_COR_Company__c > GetCompanyId( InvoiceBase invoiceBase, string currencyId, Mark mark, CancellationToken ct )
		{
			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, mark, ServiceName ), "Getting company Id for invoice #{0}".FormatWith( invoiceBase.UID ) );

			var companyInfo = new SageLiveCompanyModel
			{
				BaseCurrency = currencyId,
				LegislationId = this._pushSettings._legislationId,
				Name = ( invoiceBase is SaleInvoice ) ? this._pushSettings._companyName : invoiceBase.ContactInfo.Company
			};

			var companyMaybe = await this._asyncQueryManager.QueryOneAsync< s2cor__Sage_COR_Company__c >( SoqlQuery.Builder().Select( "Id", "Name", "s2cor__Legislation__c", "s2cor__Base_Currency__c" ).From( "s2cor__Sage_COR_Company__c" ).Where( "Name" ).IsEqualTo( this._pushSettings._companyName ), mark, ct );
			if( companyMaybe.HasValue )
			{
				if( this.AreRegionSettingsChanged( companyMaybe.Value, currencyId ) )
				{
					var updatedCompany = companyMaybe.Value;
					updatedCompany.s2cor__Legislation__c = this._pushSettings._legislationId;
					updatedCompany.s2cor__Base_Currency__c = currencyId;
					await this._asyncQueryManager.Update( new sObject[] { updatedCompany }, mark, ct );
					return updatedCompany;
				}
			}

			return await this._companyHelper.GetOrCreateCompany( companyInfo, mark, ct );
		}

		public async Task< Maybe< s2cor__Sage_INV_Trade_Document__c > > GetInvoice( InvoiceBase invoice, Mark mark, CancellationToken ct )
		{
			return ( await this._asyncQueryManager.QueryOneAsync< s2cor__Sage_INV_Trade_Document__c >( SoqlQuery.Builder().Select( "Id" ).From( "s2cor__Sage_INV_Trade_Document__c" ).Where( "s2cor__UID__c" ).IsEqualTo( invoice.UID ), mark, ct ) );
		}

		public async Task< PresentAndAbsentInvoiceInfo > GetPresentAndAbsentInvoiceInfo( IEnumerable< InvoiceBase > saleInvoices, Mark mark, CancellationToken ct )
		{
			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, mark, ServiceName ), "Getting present and absent invoices for push selection..." );
			var result = new PresentAndAbsentInvoiceInfo();
			foreach( var invoice in saleInvoices )
			{
				var dbEntry = await this.GetInvoice( invoice, mark, ct );
				if( !dbEntry.HasValue )
					result._invoicesToCreate.Add( invoice );
				//else
				//	result._invoicesToUpdate.Add( new KeyValuePair< InvoiceBase, string >( invoice, dbEntry.Value.Id ) );
			}
			return result;
		}

		public async Task< sObject > CreateInvoice( InvoiceBase invoiceModel, string invoiceTypeId, string currencyId, string dimensionId, Mark mark, CancellationToken ct )
		{
			SageLiveLogger.Debug( this.GetLogPrefix( this._authInfo, mark, ServiceName ), "Creating invoice sObject for invoice #{0}".FormatWith( invoiceModel.UID ) );
			var company = await this.GetCompanyId( invoiceModel, currencyId, mark, ct );
			var invoice = new s2cor__Sage_INV_Trade_Document__c
			{
				s2cor__Description__c = invoiceModel.Description ?? "",
				s2cor__UID__c = invoiceModel.UID,
				s2cor__Company__c = company.Id,
				s2cor__Currency__c = currencyId, //company.s2cor__Base_Currency__c; 
				s2cor__Status__c = "Submitted",
				s2cor__Paid_Amount__c = invoiceModel.Total,
				s2cor__Date__c = invoiceModel.CreationDate,
				OwnerId = this._authInfo._userId._userId,
				s2cor__Account__c = ( await this._companyHelper.GetOrCreateAccount( invoiceModel, mark, ct ) ).Id,
				s2cor__Approval_Status__c = "Approved"
			};

			var documentNumber = new s2cor__Sage_ACC_Tag__c
			{
				s2cor__Company__c = company.Id,
				s2cor__Base_Credit__c = ( invoiceModel is SaleInvoice ) ? invoiceModel.Total : 0,
				s2cor__Base_Credit__cSpecified = true,
				s2cor__Dimension__c = dimensionId,
				Name = invoiceModel.UID
			};

			var documentNumberId = await this._asyncQueryManager.Insert( new sObject[] { documentNumber }, mark, ct );
			invoice.s2cor__Document_Number_Tag__c = documentNumberId.First().id;

			invoice.s2cor__Legislation__c = /*this._pushSettings._legislationId;*/ company.s2cor__Legislation__c;
			invoice.s2cor__Trade_Document_Type__c = invoiceTypeId;
			var contactMaybe = await this._companyHelper.GetOrCreateContact( invoiceModel, invoice.s2cor__Account__c, mark, ct );

			if( invoiceModel is SaleInvoice && contactMaybe.HasValue )
			{
				invoice.s2cor__Contact__c = contactMaybe.Value.Id;
			}

			return invoice;
		}

		public async Task< List< sObject > > CreateSaleInvoices( IEnumerable< InvoiceBase > saleInvoices, string salesInvoiceDocumentTypeId, string currencyId, string dimensionId, Mark mark, CancellationToken ct )
		{
			var invoices = new List< sObject >();
			foreach( var saleInvoice in saleInvoices )
			{
				invoices.Add( await this.CreateInvoice( saleInvoice, salesInvoiceDocumentTypeId, currencyId, dimensionId, mark, ct ) );
			}

			return invoices;
		}
	}
}