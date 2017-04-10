using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SageLiveAccess.sforce;
using ServiceStack;

namespace SageLiveAccess.Misc
{
	internal class PaginationManager: MethodLogging
	{
		private readonly AsyncQueryManager _asyncQueryManager;

		public PaginationManager( AsyncQueryManager asyncQueryManager )
		{
			this._asyncQueryManager = asyncQueryManager;
		}

		private const string ServiceName = "PaginationManager";

		private async Task< List< string > > DoPartitioned( Func< sObject[], Task< SaveResult[] > > action, List< sObject > source, string info, Mark mark, int size = 20 )
		{
			var chunks = source.Partition( size ).ToArray();
			SageLiveLogger.Debug( this.GetLogPrefix( null, mark, ServiceName ), "Partioning list of objects for {1}. Got {2} chunks.".FormatWith( source.MakeString(), info, chunks.Length ) );
			var results = new List< string >();
			var count = 0;
			foreach( var chunk in chunks )
			{
				count++;
				SageLiveLogger.Debug( this.GetLogPrefix( null, mark, ServiceName ), "Processing chunk {0} of {1} for {2}".FormatWith( count, chunks.Length, info ) );
				var arr = chunk.ToArray();
				var result = await action( arr );
				results.AddRange( result.Select( x => x.id ) );
			}
			return results;
		}

		public async Task< List< string > > InsertAll( List< sObject > source, Mark mark, CancellationToken ct )
		{
			return await this.DoPartitioned( objects => this._asyncQueryManager.Insert( objects, mark, ct ), source,
				"Inserting objects: {0}".FormatWith( source.MakeString() ), mark );
		}

		public async Task< List< string > > UpdateAll( List< sObject > source, Mark mark, CancellationToken ct )
		{
			return await this.DoPartitioned( objects => this._asyncQueryManager.Update( objects, mark, ct ), source,
				"Updating objects: {0}".FormatWith( source.MakeString() ), mark );
		}

		public async Task< List< T > > GetAll< T >( SoqlQueryBuilder soqlQuery, Mark mark, CancellationToken ct ) where T : class
		{
			SageLiveLogger.Debug( this.GetLogPrefix( null, mark, ServiceName ), "Processing query pagination for SOQL query: {0}".FormatWith( soqlQuery ) );
			var result = new List< T >();
			var queryResult = await this._asyncQueryManager.QueryAsync( soqlQuery, mark, ct );
			var done = false;

			if( queryResult.size > 0 )
			{
				while( !done )
				{
					var records = queryResult.records;

					SageLiveLogger.Debug( this.GetLogPrefix( null, mark, ServiceName ), "Got {0} more records for query {1}. More records present: {2}".FormatWith( records.Length, soqlQuery, queryResult.done ) );
					result.AddRange( records.Select( t => t as T ) );

					if( queryResult.done )
						done = true;
					else
						queryResult = await this._asyncQueryManager.QueryMoreAsync( queryResult.queryLocator, mark, ct );
				}
			}
			else
				SageLiveLogger.Debug( this.GetLogPrefix( null, mark, ServiceName ), "Query {0} has returned no records...".FormatWith( soqlQuery ) );
			return result;
		}
	}
}