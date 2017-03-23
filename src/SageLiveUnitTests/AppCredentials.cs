using LINQtoCSV;

namespace SageLiveUnitTests
{
	internal class AppCredentials
	{
		[ CsvColumn( Name = "ClientId", FieldIndex = 1 ) ]
		public string ClientId{ get; set; }

		[ CsvColumn( Name = "SecretId", FieldIndex = 2 ) ]
		public string SecretId{ get; set; }

		[ CsvColumn( Name = "RedirectUri", FieldIndex = 3 ) ]
		public string RedirectUri{ get; set; }
	}
}