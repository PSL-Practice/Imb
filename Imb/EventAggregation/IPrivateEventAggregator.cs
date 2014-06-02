namespace Imb.EventAggregation
{
	/// <summary>
	/// This interface is used internally by the EventAggregator to receive timing signals that schedule 
	/// batched events.
	/// </summary>
	internal interface IPrivateEventAggregator : IEventAggregator
	{
		void TimeSignalFired();
	}
}