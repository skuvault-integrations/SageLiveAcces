using System.Threading.Tasks;
using Netco.Monads;
using SageLiveAccess.Misc;
using SageLiveAccess.Models;
using SageLiveAccess.sforce;

namespace SageLiveAccess.Helpers
{
    internal class CompanyHelper
    {
        private readonly AsyncQueryManager _asyncQueryManager;

        public CompanyHelper( AsyncQueryManager asyncQueryManager )
        {
            this._asyncQueryManager = asyncQueryManager;
        }

		public async Task< Maybe< Contact > > GetOrCreateContact( InvoiceBase invoice, string accountId )
		{
			if( invoice.ContactInfo == null )
				return Maybe< Contact >.Empty;
			var nameFormat = string.Format( "{0} {1}", invoice.ContactInfo.FirstName, invoice.ContactInfo.LastName );
			var contact = await this._asyncQueryManager.QueryOneAsync< Contact >( SoqlQuery.Builder().Select( "Id" ).From( "Contact" ).Where( "Name" ).IsEqualTo( nameFormat ).And( "AccountId").IsEqualTo( accountId ) );
			if( contact.HasValue )
				return contact.Value;

			var newContact = new Contact();
			newContact.FirstName = invoice.ContactInfo.FirstName;
			newContact.LastName = invoice.ContactInfo.LastName;
			newContact.AccountId = accountId;

			await this._asyncQueryManager.Insert( new Contact[] { newContact } );
			return newContact;
		}

        public async Task< Account > GetOrCreateAccount( InvoiceBase invoice )
        {
            var acc = await this._asyncQueryManager.QueryOneAsync< Account >( SoqlQuery.Builder().Select( "Id" ).From( "Account" ).Where( "Name" ).IsEqualTo( invoice.AccountName ) );
            if( acc.HasValue )
                return acc.Value;
            var newAccount = new Account();
            newAccount.Name = invoice.AccountName;
            var result = await this._asyncQueryManager.Insert( new Account[] { newAccount } );
            newAccount.Id = result[ 0 ].id;
            return newAccount;
        }

        public async Task< s2cor__Sage_COR_Company__c > GetOrCreateCompany( SageLiveCompanyModel company )
        {
	        var c = await this._asyncQueryManager.QueryOneAsync< s2cor__Sage_COR_Company__c >( SoqlQuery.Builder().Select( "Id" ).From( "s2cor__Sage_COR_Company__c" ).Where( "Name" ).IsEqualTo( company.Name ) );
            if( c.HasValue ) return c.Value;
            var newCompany = new s2cor__Sage_COR_Company__c();
            newCompany.Name = company.Name;
            newCompany.s2cor__Legislation__c = company.LegislationId;
            newCompany.s2cor__Base_Currency__c = company.BaseCurrency;
            var result = await this._asyncQueryManager.Insert( new s2cor__Sage_COR_Company__c[] { newCompany } );
            newCompany.Id = result[ 0 ].id;
            return newCompany; 
        }


    }
}