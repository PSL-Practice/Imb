using System.Threading;
using System.Windows.Threading;

namespace Imb.EventAggregation
{
	internal abstract class GenericListenerContainer
	{
		public abstract void Call(object message);

		private Dispatcher _dispatcher;
		public virtual Dispatcher Dispatcher
		{
			get { return _dispatcher; }
			set { _dispatcher = value; }
		}

		public SynchronizationContext SyncContext = null;

		public abstract object Listener { get; }

		public virtual void CallInCurrentContext(object message)
		{
			
		}
	}
}