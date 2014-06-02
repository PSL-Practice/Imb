using System.Threading;
using System.Windows.Threading;

namespace Imb.EventAggregation
{
	/// <summary>
	/// This is the public interface of an event aggregator
	/// </summary>
	public interface IEventAggregator
	{
		/// <summary>
		/// Send a constructed message.
		/// </summary>
		/// <typeparam name="T">The message type</typeparam>
		/// <param name="message">The message instance</param>
		void SendMessage<T>(T message);

		/// <summary>
		/// Send a message that does not require initialisation.
		/// </summary>
		/// <typeparam name="T">The message type</typeparam>
		void SendMessage<T>() where T : new();

		/// <summary>
		/// Register a class as a listener. Events will be dispatched on the sending thread. This is an 
		/// important concept, in that if you subscribe on the UI thread without specifying a 
		/// <see cref="SynchronizationContext"/> then the callbacks will be called from the message 
		/// sender's thread. If the handler accesses UI objects, it is up to the developer to ensure 
		/// the callbacks originate on the correct thread.
		/// </summary>
		/// <param name="listener">The object subscribing to events.</param>
		void AddListener(object listener);

		/// <summary>
		/// Register a class as a listener, supplying a <see cref="SynchronizationContext"/> to be used
		/// to call back when events occur. This means the callbacks will occur via the context regardless
		/// of the thread sending the message.
		/// </summary>
		/// <param name="listener">The object subscribing to events.</param>
		/// <param name="synchronizationContext">The context to be used to callback the object.</param>
		void AddListener(object listener, SynchronizationContext synchronizationContext);

		/// <summary>
		/// Register a class as a listener, supplying a <see cref="Dispatcher"/> to be used
		/// to call back when events occur. This means the callbacks will occur via the dispatcher regardless
		/// of the thread sending the message.
		/// </summary>
		/// <param name="listener">The object subscribing to events.</param>
		/// <param name="dispatcher">The Dispatcher instance that should be used to invoke the message handlers</param>
		void AddListener(object listener, Dispatcher dispatcher);

		/// <summary>
		/// Prevent callbacks on this object. The object will be removed from the listeners collection
		/// during this call, but messages can be sent from any thread and may be "in-flight" when the
		/// object is removed.
		/// </summary>
		/// <param name="listener">The object unsubscribing from events.</param>
		void RemoveListener(object listener);
	}
}
