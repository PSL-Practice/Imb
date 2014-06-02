using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace Imb.EventAggregation
{
	/// <summary>
	/// This is the standard implementation of <see cref="IEventAggregator"/>.
	/// </summary>
	public class EventAggregator : IPrivateEventAggregator
	{
		private readonly Dictionary<Type, List<GenericListenerContainer>> _listeners = new Dictionary<Type, List<GenericListenerContainer>>();
		private MessageBatcher _batcher;
		private AggregatorTimeSignalGenerator _timer;
		private readonly int _batchInterval;

		public EventAggregator(int batchInterval = 100)
		{
			_batchInterval = batchInterval;
		}

		public int BatchesProcessed
		{
			get
			{
				return _batcher != null ? _batcher.NumBatchesProcessed : 0;
			}
		}

		#region Implementation of IEventAggregator

		public void SendMessage<T>(T message)
		{
			lock (_listeners)
			{
				var messageType = typeof(T);
				if (_listeners.ContainsKey(messageType))
				{
					foreach (var listener in _listeners[messageType])
					{
						listener.Call(message);
					}
				}
			}
		}

		public void SendMessage<T>() where T : new()
		{
			SendMessage(new T());
		}

		public void AddListener(object listener)
		{
			AddListener(listener, null, null);
		}

		public void AddListener(object listener, Dispatcher dispatcher)
		{
			AddListener(listener, dispatcher, null);
		}

		public void AddListener(object listener, SynchronizationContext synchronizationContext)
		{
			AddListener(listener, null, synchronizationContext);
		}

		private void AddListener(object listener, Dispatcher dispatcher, SynchronizationContext synchronizationContext)
		{
			var genericContainerType = typeof(ListenerContainer<>);
			var batchingContainerType = typeof(BatchingListenerContainer<>);
			var listenerType = listener.GetType();
			var listenerTypes = InterfaceExtractor.GetImplementationsOfGenericInterface(listenerType, typeof(IListener<>));
			var constructorParams = new[] { listener };
			var batchConstructorParams = new[] { listener, _batcher };
			lock (_listeners)
			{
				foreach (var interfaceType in listenerTypes)
				{
					Type containerType;
					GenericListenerContainer instance;
					var genericArguments = interfaceType.GetGenericArguments();
					if (genericArguments.Length > 0 && genericArguments[0].GetCustomAttributes(typeof(BatchedMessage), true).Any())
					{
						if (_batcher == null)
							batchConstructorParams[1] = MakeBatcher();

						containerType = batchingContainerType.MakeGenericType(genericArguments);
						instance = Activator.CreateInstance(containerType, batchConstructorParams) as GenericListenerContainer;
					}
					else
					{
						containerType = genericContainerType.MakeGenericType(genericArguments);
						instance = Activator.CreateInstance(containerType, constructorParams) as GenericListenerContainer;
					}

					if (instance != null)
					{
						instance.Dispatcher = dispatcher;
						instance.SyncContext = synchronizationContext;

						if (_listeners.ContainsKey(genericArguments[0]))
							_listeners[genericArguments[0]].Add(instance);
						else
							_listeners[genericArguments[0]] = new List<GenericListenerContainer> { instance };
					}
				}
			}
		}

		private MessageBatcher MakeBatcher()
		{
			lock (this)
			{
				if (_batcher == null)
				{
					_timer = new AggregatorTimeSignalGenerator(this, _batchInterval);
					_batcher = new MessageBatcher();
				}
			}
			return _batcher;
		}

		public void RemoveListener(object listener)
		{
			lock (_listeners)
			{
				foreach (var listenerContainers in _listeners.Values)
				{
					var matches = listenerContainers.Where(i => ReferenceEquals(i.Listener, listener)).ToList();
					foreach (var match in matches)
						listenerContainers.Remove(match);
				}
			}
		}

		public void TimeSignalFired()
		{
			_batcher.Dispatch();
		}

		#endregion

		public List<Type> GetMessageTypesHandled(object listener)
		{
			lock (_listeners)
			{
				return
					(from l in _listeners where l.Value.Where(s => ReferenceEquals(s.Listener, listener)).Any() select l.Key).ToList();
			}
		}
	}
}
