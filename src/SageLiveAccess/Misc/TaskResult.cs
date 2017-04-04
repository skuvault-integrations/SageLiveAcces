using System;

namespace SageLiveAccess.Misc
{
	internal class TaskResult< T >
	{
		public bool IsSuccess{ get; }
		public Exception Exception{ get; }
		public T Result{ get; }

		public TaskResult( bool isSuccess, Exception ex, T result )
		{
			this.IsSuccess = isSuccess;
			this.Exception = ex;
			this.Result = result;
		}
	}
}
