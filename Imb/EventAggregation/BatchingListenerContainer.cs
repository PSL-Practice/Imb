using System;
using System.Windows.Threading;

namespace Imb.EventAggregation
{
	/// <summary>
	/// This class is a <see cref="GenericListenerContainer"/> for messages that must be dispatched in
	/// batches. The class is associated with a <see cref="MessageBatcher"/> that performs the actual
	/// batching, and the <see cref="BatchingListenerContainer{T}"/> simple requests that the message
	/// be added to the next batch.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class BatchingListenerContainer<T> : GenericListenerContainer where T : class
	{
		private readonly MessageBatcher _batcher;
		private readonly WeakReference _listener;
		private ListenerContainer<T> _listenerContainer; 

		public BatchingListenerContainer(object listener, MessageBatcher batcher)
		{
			_batcher = batcher;
			_listener = new WeakReference(listener as IListener<T>);
			_listenerContainer = new ListenerContainer<T>(listener);
		}

		#region Overrides of GenericListenerContainer

		public override void Call(object message)
		{
			_batcher.AddMessage(message, _listenerContainer);
		}

		public override Dispatcher Dispatcher
		{
			get { return base.Dispatcher; }
			set
			{
				if (_listenerContainer != null)
					_listenerContainer.Dispatcher = value;

				base.Dispatcher = value;
			}
		}

		public override object Listener
		{
			get { return _listener.Target; }
		}

		#endregion
	}
}