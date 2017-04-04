using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Netco.Extensions;
using Netco.Monads;
using SageLiveAccess.Helpers;
using SageLiveAccess.Models.Auth;
using SageLiveAccess.sforce;
using Task = System.Threading.Tasks.Task;

namespace SageLiveAccess.Misc
{
	internal class AsyncQueryManager: MethodLogging
	{
		private readonly SforceService _binding;
		private readonly string _refreshToken;
		private readonly SageLiveReAuthService _reAuthService;
		private readonly Guid _instanceId = Guid.NewGuid();
		private const string ServiceName = "AsyncQueryManager";

		public AsyncQueryManager( SforceService binding, SageLiveFactoryConfig config, string refreshToken )
		{
			this._binding = binding;
			this._binding.queryCompleted += this.Binding_queryCompleted;
			this._binding.queryMoreCompleted += this.Binding_queryMoreCompleted;
			this._binding.createCompleted += this.BindingOnCreateCompleted;
			this._binding.updateCompleted += this.BindingOnUpdateCompleted;
			this._binding.deleteCompleted += this.BindingOnDeleteCompleted;

			this._reAuthService = new SageLiveReAuthService( config );
			this._refreshToken = refreshToken;
		}

		private async Task< T > RetryWrapper< T >( Action< TaskIdentifier< T > > f, string info, Mark mark, CancellationToken ct )
		{
			return await ActionPolicies.GetAsyncQueryManagerActionPolicy( this, this.GetLogPrefix( null, mark, ServiceName ), info, mark ).Get( async () =>
			{
				SageLiveLogger.Debug( this.GetLogPrefix( null, mark, ServiceName ), "Trying with Salesforce SOAP API request {0}".FormatWith( info ) );
				var indentifier = new TaskIdentifier< T >( this._instanceId, new SemaphoreSlim( 0, 1 ), mark );
				SecurityHelper.SetSecurityProtocol();
				f.Invoke( indentifier );
				await indentifier.CallbackMonitor.WaitAsync( ct );
				if( indentifier.Result.IsSuccess )
				{
					SageLiveLogger.Debug( this.GetLogPrefix( null, mark, ServiceName ), "Salesforce SOAP API request {0} succeeded".FormatWith( info ) );
					return indentifier.Result.Result;
				}
				if( IsSessionExpired( indentifier.Result.Exception ) )
				{
					throw new SessionExpiredException( "Your session has expired and needs to be refreshed", indentifier.Result.Exception );
				}

				throw new AggregateException( "Error occured while trying to execute SOAP API request {0}".FormatWith( info ), new List< Exception > { indentifier.Result.Exception } );
			} );
		}

		#region Queries
		public async Task< SaveResult[] > Insert( sObject[] objects, Mark mark, CancellationToken ct )
		{
			return await this.RetryWrapper< SaveResult[] >( t =>
			{
				this._binding.createAsync( objects, t );
			}, "Inserting to SOQL: {0}".FormatWith( objects.MakeString() ), mark, ct );
		}

		public async Task< SaveResult[] > Update( sObject[] objects, Mark mark, CancellationToken ct )
		{
			return await this.RetryWrapper< SaveResult[] >( t =>
			{
				this._binding.updateAsync( objects, t );
			}, "Updating in SOQL: {0}".FormatWith( objects.MakeString() ), mark, ct );
		}

		public async Task Delete( string[] ids, Mark mark, CancellationToken ct )
		{
			await this.RetryWrapper< DeleteResult[] >( t =>
			{
				this._binding.deleteAsync( ids, t );
			}, "Deleting from SOQL: {0}".FormatWith( ids.MakeString() ), mark, ct );
		}

		public async Task< QueryResult > QueryAsync( SoqlQueryBuilder builder, Mark mark, CancellationToken ct )
		{
			var soqlQuery = builder.Build();
			return await this.RetryWrapper< QueryResult >( t =>
			{
				this._binding.queryAsync( soqlQuery, t );
			}, "Quering from SOQL: {0}".FormatWith( soqlQuery ), mark, ct );
		}

		public async Task< Maybe< T > > QueryOneAsync< T >( SoqlQueryBuilder builder, Mark mark, CancellationToken ct ) where T : class
		{
			var query = builder.Build();
			var result = await this.RetryWrapper< QueryResult >( t =>
			{
				this._binding.queryAsync( query, t );
			}, "Quering single record from SOQL: {0}".FormatWith( query ), mark, ct );
			if( result.records == null )
				return Maybe< T >.Empty;
			return result.records.Length > 0 ? result.records[ 0 ] as T : Maybe< T >.Empty;
		}

		public async Task< QueryResult > QueryMoreAsync( string query, Mark mark, CancellationToken ct )
		{
			return await this.RetryWrapper< QueryResult >( t =>
			{
				this._binding.queryMoreAsync( query, t );
			}, "Quering more from SOQL: {0}".FormatWith( query ), mark, ct );
		}
		#endregion

		#region Events
		private void Binding_queryCompleted( object sender, queryCompletedEventArgs e )
		{
			this.BindingCommandCompleted( e.UserState, e.Error, e.Error == null ? e.Result : null, "Query completed callback executing." );
		}

		private void Binding_queryMoreCompleted( object sender, queryMoreCompletedEventArgs e )
		{
			this.BindingCommandCompleted( e.UserState, e.Error, e.Error == null ? e.Result : null, "QueryMore completed callback executing." );
		}

		private void BindingOnCreateCompleted( object sender, createCompletedEventArgs e )
		{
			this.BindingCommandCompleted( e.UserState, e.Error, e.Error == null ? e.Result : null, "Insert completed callback executing." );
		}

		private void BindingOnUpdateCompleted( object sender, updateCompletedEventArgs e )
		{
			this.BindingCommandCompleted( e.UserState, e.Error, e.Error == null ? e.Result : null, "Update completed callback executing." );
		}

		private void BindingOnDeleteCompleted( object sender, deleteCompletedEventArgs e )
		{
			this.BindingCommandCompleted( e.UserState, e.Error, e.Error == null ? e.Result : null, "Delete completed callback executing." );
		}

		private void BindingCommandCompleted< T >( object userState, Exception error, T commandResult, string message ) where T : class
		{
			if( !( userState is TaskIdentifier< T > ) || !( ( TaskIdentifier< T > )userState ).TaskGUID.Equals( this._instanceId ) )
				return;

			var taskIdentifier = ( TaskIdentifier< T > )userState;
			var result = new TaskResult< T >(
				error == null,
				error,
				error == null ? commandResult : null
			);

			SageLiveLogger.Debug( this.GetLogPrefix( null, taskIdentifier.Mark, ServiceName ), message + " Success = {0}".FormatWith( error == null ) );
			taskIdentifier.Result = result;
			taskIdentifier.CallbackMonitor.Release();
		}
		#endregion

		#region Session
		private static bool IsSessionExpired( Exception exception )
		{
			if( !( exception is System.Web.Services.Protocols.SoapException ) )
				return false;
			var soapException = ( System.Web.Services.Protocols.SoapException )exception;
			return soapException.Code.Name == "INVALID_SESSION_ID";
		}

		internal async Task< bool > RefreshTokenIfNeeded( Mark mark )
		{
			SageLiveLogger.Debug( this.GetLogPrefix( null, mark, ServiceName ), "Session token expired. Refreshing..." );

			var newSessionId = await this._reAuthService.GetRefreshedToken( this._refreshToken, mark );
			this._binding.SessionHeaderValue.sessionId = newSessionId;
			return true;
		}
		#endregion
	}
}