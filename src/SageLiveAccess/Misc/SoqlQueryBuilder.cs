using System;
using System.Collections.Generic;

namespace SageLiveAccess.Misc
{
	public static class SoqlQuery
	{
		public static SoqlQueryBuilder Builder()
		{
			return new SoqlQueryBuilder();
		}
	}

	public class SoqlQueryBuilder
	{
		private List< string > segments = new List< string >();

		public SoqlQueryBuilder Select( params string[] fields )
		{
			this.segments.Add( "SELECT" );
			this.segments.Add( fields.MakeString() );
			return this;
		}

		public SoqlQueryBuilder From( string tableName )
		{
			this.segments.Add( "FROM" );
			this.segments.Add( tableName );
			return this;
		} 

		public SoqlQueryBuilder Where( string fieldName )
		{
			this.segments.Add( "WHERE" );
			this.segments.Add( fieldName );
			return this;
		}

		private static string GetEscapedValue( string value )
		{
			if( string.IsNullOrEmpty( value ) )
				return value;
			var escapedQuotes = value.Replace( "'", @"\'" );
			return escapedQuotes;
		}

		public SoqlQueryBuilder IsEqualTo( string value )
		{
			this.segments.Add( "=" );
			this.segments.Add( string.Format( "'{0}'", GetEscapedValue( value ) ) );
			return this;
		}

		private static string FormatDate( DateTime dateTime )
		{
			return dateTime.ToString( "yyyy-MM-ddTHH:mm:ssZ" );
		}

		public SoqlQueryBuilder IsLessThan( DateTime value )
		{
			this.segments.Add( "<" );
			this.segments.Add( FormatDate( value ) );
			return this;
		}

		public SoqlQueryBuilder IsGreaterThan( DateTime value )
		{
			this.segments.Add( ">" );
			this.segments.Add( FormatDate( value ) );
			return this;
		}

		public SoqlQueryBuilder And( string fielName )
		{
			this.segments.Add( "AND" );
			this.segments.Add( fielName );
			return this;
		}

		public string Build()
		{
			return string.Join( " ", this.segments );
		}
	}
}
