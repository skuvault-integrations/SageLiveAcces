using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SageLiveAccess;
using SageLiveAccess.Misc;
using SageLiveAccess.Models;
using SageLiveAccess.Models.Auth;

namespace SageLiveUnitTests
{
	[ TestClass ]
	public class SageLiveUnitTest
	{
		private SageLiveFactory _factory;
		private SageLiveAuthInfo _authInfo;

		private static string[] LoadCredentials()
		{
			var reader = new StreamReader( File.OpenRead( @"D:\sagelive.csv" ) );
			var line = reader.ReadLine();
			var values = line.Split( ';' );
			return values;
		}

		[ TestInitialize ]
		public void Setup()
		{
			var credentials = LoadCredentials();

			this._factory = new SageLiveFactory( credentials[ 0 ], credentials[ 1 ], credentials[ 2 ] );
			this._authInfo = new SageLiveAuthInfo(
				new SageLiveSessionId( credentials[ 3 ] ),
				new SageLiveOrganizationId( credentials[ 4 ] ),
				new SageLiveUserId( credentials[ 5 ] ),
				new SageLiveInstanceUrl( credentials[ 6 ] ),
				new SageLiveRefreshToken( credentials[ 7 ] )
			);
		}

		[ TestMethod ]
		public void AuthentifcateByCodeTest()
		{
			var authService = this._factory.CreateSageLiveAuthService();
			var authUrl = authService.GetAuthUrl();
			var token = "";
			var authInfo = authService.AuthentifcateByCode( token );
		}

		[ TestMethod ]
		public void InvoiceGetTest()
		{
			var service = this._factory.CreateSageLiveSaleInvoiceSyncService( this._authInfo, new SageLivePushInvoiceSettings( "a1B580000006bM9EAI", "AnythingCompany" ), "USD" );
			var now = DateTime.UtcNow;

			var x = service.GetSaleInvoices( now.AddHours( -12 ), now, CancellationToken.None ).Result;
			Assert.AreEqual( true, true );
		}

		[ TestMethod ]
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

		[ TestMethod ]
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

		[ TestMethod ]
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