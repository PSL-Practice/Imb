namespace Imb.EventAggregation
{
	/// <summary>
	/// Listeners must implement this interface in order to receive messages from an EventAggregator.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IListener<in T> 
	{
		/// <summary>
		/// Called when a message of the subscribed type is received.
		/// </summary>
		/// <param name="message"></param>
		void Handle(T message);
	}
}