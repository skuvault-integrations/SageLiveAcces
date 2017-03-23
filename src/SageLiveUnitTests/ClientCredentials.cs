using LINQtoCSV;

namespace SageLiveUnitTests
{
	internal class ClientCredentials
	{
		[ CsvColumn( Name = "RefreshToken", FieldIndex = 1 ) ]
		public string RefreshToken{ get; set; }

		[ CsvColumn( Name = "InstanceUrl", FieldIndex = 2 ) ]
		public string InstanceUrl{ get; set; }

		[ CsvColumn( Name = "LegislationId", FieldIndex = 3 ) ]
		public string LegislationId{ get; set; }

		[ CsvColumn( Name = "CompanyName", FieldIndex = 4 ) ]
		public string CompanyName{ get; set; }

		[ CsvColumn( Name = "OrganizationId", FieldIndex = 5 ) ]
		public string OrganizationId{ get; set; }

		[ CsvColumn( Name = "UserId", FieldIndex = 6 ) ]
		public string UserId{ get; set; }

		[ CsvColumn( Name = "SessionId", FieldIndex = 7 ) ]
		public string SessionId{ get; set; }
	}
}