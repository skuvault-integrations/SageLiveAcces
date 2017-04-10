using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LINQtoCSV;
using NUnit.Framework;
using SageLiveAccess;
using SageLiveAccess.Misc;
using SageLiveAccess.Models;
using SageLiveAccess.Models.Auth;
using Assert = NUnit.Framework.Assert;

namespace SageLiveUnitTests
{
	public class SageLiveUnitTests
	{
		private SageLiveFactory _factory;
		private SageLiveAuthInfo _authInfo;
		private ClientCredentials _clientCredentials;

		[ SetUp ]
		public void Setup()
		{
			var cc = new CsvContext();
			var appCredentials = cc.Read< AppCredentials >( @"..\..\Files\sageliveAppCredentials.csv", new CsvFileDescription { FirstLineHasColumnNames = true } ).FirstOrDefault();
			this._clientCredentials = cc.Read< ClientCredentials >( @"..\..\Files\sageliveClientCredentials.csv", new CsvFileDescription { FirstLineHasColumnNames = true } ).FirstOrDefault();

			if( appCredentials != null )
				this._factory = new SageLiveFactory( appCredentials.ClientId, appCredentials.SecretId, appCredentials.RedirectUri );
			if( this._clientCredentials != null )
				this._authInfo = new SageLiveAuthInfo(
					new SageLiveSessionId( this._clientCredentials.SessionId ),
					new SageLiveOrganizationId( this._clientCredentials.OrganizationId ),
					new SageLiveUserId( this._clientCredentials.UserId ),
					new SageLiveInstanceUrl( this._clientCredentials.InstanceUrl ),
					new SageLiveRefreshToken( this._clientCredentials.RefreshToken )
				);
		}

		[ Test ]
		public void AuthentifcateByCodeTest()
		{
			var authService = this._factory.CreateSageLiveAuthService();
			var authUrl = authService.GetAuthUrl();
			var token = "";
			var authInfo = authService.AuthentifcateByCode( token );
		}

		[ Test ]
		public async Task GetLegislationInfo()
		{
			var authService = this._factory.CreateSageLiveSettingsService( this._authInfo );
			var legresult = await authService.GetLegislationInfo( CancellationToken.None );
			Assert.IsNotEmpty( legresult.legislations );
		}

		[ Test ]
		public void InvoiceGetTest()
		{
			var service = this._factory.CreateSageLiveSaleInvoiceSyncService( this._authInfo, new SageLivePushInvoiceSettings( this._clientCredentials.LegislationId, this._clientCredentials.CompanyName ), "USD" );
			var now = DateTime.UtcNow;

			var x = service.GetSaleInvoices( now.AddDays( -3 ), now, CancellationToken.None ).Result;

			Assert.AreEqual( true, true );
		}

		[ Test ]
		public void InvoiceCreateTest()
		{
			var salesInvoice = new SaleInvoice
			{
				UID = "LETITBE-8F4D4DK6",
				AddressInfo = new AddressInfo
				{
					City = "Ufa",
					State = "Bashkortostan",
					Street = "Sesame",
					Zip = "111111222222"
				},
				ContactInfo = new ContactInfo
				{
					Company = "New company",
					FirstName = "Bob",
					LastName = "Brown"
				},
				CreationDate = DateTime.Now,
				FulfilledBy = "3PL",
				Items = new List< InvoiceItem >
				{
					new InvoiceItem
					{
						ProductCode = "CODEN1",
						ProductName = "New Product",
						ProductUID = "SLAKD11",
						Quantity = 1,
						UnitPrice = 3.50
					}
				},
				LastModifiedDate = DateTime.Now,
				Status = "Unsubmitted",
				AccountName = "NewAccount"
			};

			var service = this._factory.CreateSageLiveSaleInvoiceSyncService( this._authInfo, new SageLivePushInvoiceSettings( "a1B580000006bM9EAI", "Anything's Company" ), "XYZ" );
			service.PushSaleInvoices( new List< SaleInvoice > { salesInvoice }, CancellationToken.None ).Wait();
			Assert.AreEqual( true, true );
		}

		[ Test ]
		public void InvoiceCreateTestPurchase()
		{
			var purchaseInvoice = new PurchaseInvoice
			{
				UID = "LETITBE-EMPTY4",
				AddressInfo = new AddressInfo
				{
					City = "Ufa",
					State = "Bashkortostan",
					Street = "Sesame",
					Zip = "111111222222"
				},
				ContactInfo = new ContactInfo
				{
					Company = "New company",
					FirstName = "Bob",
					LastName = "Brown"
				},
				CreationDate = DateTime.Now,
				FulfilledBy = "3PL",
				Items = new List< InvoiceItem >
				{
					new InvoiceItem
					{
						ProductCode = "CODEN1NNN",
						ProductName = "",
						ProductUID = "SLAKD11NN",
						Quantity = 1,
						UnitPrice = 3.50
					}
				},
				LastModifiedDate = DateTime.Now,
				Status = "Unsubmitted",
				AccountName = "NewAccount"
			};

			var service = this._factory.CreateSageLivePurchaseInvoiceSyncService( this._authInfo, new SageLivePushInvoiceSettings( "a1B580000006bM9EAI", "Anything's Company" ), "USD" );
			service.PushPurchaseInvoices( new List< PurchaseInvoice > { purchaseInvoice }, CancellationToken.None ).Wait();
			Assert.AreEqual( true, true );
		}

		[ Test ]
		public void SoqlQueryBuilderTest()
		{
			var builder = new SoqlQueryBuilder();
			var query = builder.Select( "Id", "Name", "Email" )
				.From( "Accounts" )
				.Where( "Age" ).IsEqualTo( "20" ).And( "Name" ).IsEqualTo( "Mc'Dss" );
			Assert.AreEqual( query.Build(), @"SELECT Id, Name, Email FROM Accounts WHERE Age = '20' AND Name = 'Mc\'Dss'" );
		}
	}
}