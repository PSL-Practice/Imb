using System;
using System.Collections.Generic;
using System.Linq;

namespace Imb.EventAggregation
{
	/// <summary>
	/// This class is responsible for batching messages when <see cref="AddMessage"/> is called, 
	/// and dispatching the batch when the <see cref="Dispatch"/> method is called.
	/// </summary>
	internal class MessageBatcher
	{
		private class BatchedMessageWrapper
		{
			public object Message { get; private set; }
			public GenericListenerContainer ListenerContainer { get; private set; }

			public BatchedMessageWrapper(object message, GenericListenerContainer listenerContainer)
			{
				Message = message;
				ListenerContainer = listenerContainer;
			}

			public void ExecuteInCurrentContext()
			{
				ListenerContainer.CallInCurrentContext(Message);
			}
		}

		private List<BatchedMessageWrapper> _batch = new List<BatchedMessageWrapper>();

		public int NumBatchesProcessed { get; set; }

		public void Dispatch()
		{
			List<BatchedMessageWrapper> dispatchBatch = null;
			lock (this)
			{
				dispatchBatch = _batch;
				_batch = new List<BatchedMessageWrapper>();
			}
			if (dispatchBatch.Any())
				SendBatch(dispatchBatch);

		}

		private void SendBatch(List<BatchedMessageWrapper> dispatchBatch)
		{
			var dispatchers =
				dispatchBatch.Where(d => d.ListenerContainer.Dispatcher != null).Select(w => w.ListenerContainer.Dispatcher).
					Distinct();
			var syncContexts =
				dispatchBatch.Where(d => d.ListenerContainer.SyncContext != null).Select(w => w.ListenerContainer.SyncContext).
					Distinct();

			foreach (var dispatcher in dispatchers)
			{
				dispatcher.BeginInvoke(new Action(() => ExecuteBatch(dispatchBatch.Where(w => w.ListenerContainer.Dispatcher == dispatcher))));
			}

			foreach (var syncContext in syncContexts)
			{
				syncContext.Post((_) => ExecuteBatch(dispatchBatch.Where(w => w.ListenerContainer.SyncContext == syncContext)), null);
			}

			foreach (var messageWrapper in dispatchBatch.Where(w => w.ListenerContainer.SyncContext == null && w.ListenerContainer.Dispatcher == null))
			{
				messageWrapper.ExecuteInCurrentContext();
			}
			++NumBatchesProcessed;
		}

		private void ExecuteBatch(IEnumerable<BatchedMessageWrapper> batchedMessageWrappers)
		{
			foreach (var batchedMessageWrapper in batchedMessageWrappers)
			{
				batchedMessageWrapper.ExecuteInCurrentContext();
			}
		}

		public void AddMessage(object message, GenericListenerContainer container)
		{
			lock (this)
			{
				_batch.Add(new BatchedMessageWrapper(message, container));
			}
		}
	}
}