using System;
using System.Threading;

namespace SageLiveAccess.Misc
{
	internal class TaskIdentifier< T >
	{
		public Guid TaskGUID{ get; }
		public TaskResult< T > Result{ get; set; }
		public SemaphoreSlim CallbackMonitor{ get; }
		public Mark Mark{ get; }

		public TaskIdentifier( Guid instanceId, SemaphoreSlim callbackMonitor, Mark mark )
		{
			this.TaskGUID = instanceId;
			this.CallbackMonitor = callbackMonitor;
			this.Mark = mark;
		}
	}
}
