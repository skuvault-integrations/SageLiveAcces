using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SageLiveAccess.sforce;
using ServiceStack;

namespace SageLiveAccess.Misc
{
	internal class PaginationManager : MethodLogging
	{
		private readonly AsyncQueryManager _asyncQueryManager;

		public PaginationManager( AsyncQueryManager asyncQueryManager )
		{
			this._asyncQueryManager = asyncQueryManager;
		}

		private const string ServiceName = "PaginationManager";

		private async Task< List< string> > DoPartitioned( Func< sObject[], Task< SaveResult[] > > action, List<sObject> source, string info )
		{
			var chunks = source.Partition( 20 );
			SageLiveLogger.Debug( this.GetLogPrefix( null, ServiceName ), "Partioning list of objects for {1}. Got {2} chunks.".FormatWith( source.MakeString(), info, chunks.Count() ) );
			var results = new List<string>();
			var count = 0;
			foreach ( var chunk in chunks )
			{
				count++;
				SageLiveLogger.Debug( this.GetLogPrefix( null, ServiceName ), "Processing chunk {0} of {1} for {2}".FormatWith( count, chunks.Count(), info ) );
				var arr = chunk.ToArray();
				var result = await action( arr );
				results.AddRange( result.Select( x => x.id ) );
			}
			return results;
		}

		public async Task< List< string > > InsertAll( List< sObject > source )
		{
			return await this.DoPartitioned( objects => this._asyncQueryManager.Insert( objects ), source,
				"Inserting objects: {0}".FormatWith( source.MakeString() ) );
		}

		public async Task< List< string > > UpdateAll( List<sObject> source )
		{
			return await this.DoPartitioned( objects => this._asyncQueryManager.Update( objects ), source,
				"Updating objects: {0}".FormatWith( source.MakeString() ) );
		}

		public async Task< List< T > > GetAll< T >( SoqlQueryBuilder soqlQuery ) where T : class
		{
			SageLiveLogger.Debug( this.GetLogPrefix( null, ServiceName ), "Processing query pagination for SOQL query: {0}".FormatWith( soqlQuery ) );
			var result = new List< T >();
			var qr = await this._asyncQueryManager.QueryAsync( soqlQuery );
			bool done = false;

			if( qr.size > 0 )
			{
				while( !done )
				{
					sObject[] records = qr.records;

					SageLiveLogger.Debug( this.GetLogPrefix( null, ServiceName ), "Got {0} more records for query {1}. More records present: {2}".FormatWith( records.Length, soqlQuery, qr.done ) );
					for ( int i = 0; i < records.Length; i++ )
					{
						var doc = records[ i ] as T;
						result.Add( doc );
					}

					if( qr.done )
						done = true;
					else
						qr = await this._asyncQueryManager.QueryMoreAsync( qr.queryLocator );
				}
			}
			else
				SageLiveLogger.Debug( this.GetLogPrefix( null, ServiceName ), "Query {0} has returned no records...".FormatWith( soqlQuery ) );
			return result;
		}
	}
}