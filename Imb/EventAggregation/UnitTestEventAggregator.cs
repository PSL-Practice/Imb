using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace Imb.EventAggregation
{
	/// <summary>
	/// This is a test implementation of the IEventAggregator interface allowing messages to be sent
	/// as part of the execution of a unit test.
	/// </summary>
	public class UnitTestEventAggregator : IEventAggregator
	{
		private readonly List<WeakReference> _listeners = new List<WeakReference>();

		#region Implementation of IEventAggregator

		/// <summary>
		/// Send a constructed message.
		/// </summary>
		/// <typeparam name="T">The message type</typeparam>
		/// <param name="message">The message instance</param>
		public void SendMessage<T>(T message)
		{
			foreach (IListener<T> listener in
				_listeners.Where(l => l.Target is IListener<T>).Select(l => l.Target).ToList())
			{
				listener.Handle(message);
			}
		}

		/// <summary>
		/// Send a message that does not require initialisation.
		/// </summary>
		/// <typeparam name="T">The message type</typeparam>
		public void SendMessage<T>() where T : new()
		{
			SendMessage(new T());
		}

		/// <summary>
		/// Register a class as a listener. Events will be dispatched on the sending thread. This is an 
		/// important concept, in that if you subscribe on the UI thread without specifying a 
		/// <see cref="SynchronizationContext"/> then the callbacks will be called from the message 
		/// sender's thread. If the handler accesses UI objects, it is up to the developer to ensure 
		/// the callbacks originate on the correct thread.
		/// </summary>
		/// <param name="listener">The object subscribing to events.</param>
		public void AddListener(object listener)
		{
			_listeners.Add(new WeakReference(listener));
		}

		/// <summary>
		/// Register a class as a listener, supplying a <see cref="SynchronizationContext"/> to be used
		/// to call back when events occur. This means the callbacks will occur via the context regardless
		/// of the thread sending the message.
		/// </summary>
		/// <param name="listener">The object subscribing to events.</param>
		/// <param name="synchronizationContext">The context to be used to callback the object.</param>
		public void AddListener(object listener, SynchronizationContext synchronizationContext)
		{
			AddListener(listener);
		}

		/// <summary>
		/// Register a class as a listener, supplying a <see cref="Dispatcher"/> to be used
		/// to call back when events occur. This means the callbacks will occur via the dispatcher regardless
		/// of the thread sending the message.
		/// </summary>
		/// <param name="listener">The object subscribing to events.</param>
		/// <param name="dispatcher">The Dispatcher instance that should be used to invoke the message handlers</param>
		public void AddListener(object listener, Dispatcher dispatcher)
		{
			AddListener(listener);
		}

		/// <summary>
		/// Prevent callbacks on this object. The object will be removed from the listeners collection
		/// during this call, but messages can be sent from any thread and may be "in-flight" when the
		/// object is removed.
		/// </summary>
		/// <param name="listener">The object unsubscribing from events.</param>
		public void RemoveListener(object listener)
		{
			var matches = _listeners.Where(l => l.Target == null || ReferenceEquals(l.Target, listener)).ToList();
			foreach (var match in matches)
				_listeners.Remove(match);
		}

		#endregion
	}

}
