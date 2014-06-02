using System;

namespace Imb.EventAggregation
{
	internal class ListenerContainer<T> : GenericListenerContainer where T : class
	{
		private readonly WeakReference _listener;

		public ListenerContainer(object listener)
		{
			_listener = new WeakReference(listener as IListener<T>);
		}

		#region Overrides of GenericListenerContainer

		public override void Call(object message)
		{
			var listener = _listener.Target as IListener<T>;
			if (listener != null)
			{
				if (Dispatcher != null)
				{
					Dispatcher.BeginInvoke(new Action(() => listener.Handle(message as T)), null);
				}
				else if (SyncContext == null)
					listener.Handle(message as T);
				else
					SyncContext.Post(o => ((IListener<T>)o).Handle(message as T), listener);
			}
		}

		public override object Listener
		{
			get { return _listener.Target; }
		}

		public override void CallInCurrentContext(object message)
		{
			var listener = _listener.Target;
			if (listener != null)
				((IListener<T>)listener).Handle(message as T);
		}

		#endregion
	}
}