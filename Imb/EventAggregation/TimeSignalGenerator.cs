namespace Imb.EventAggregation
{
	/// <summary>
	/// This class runs a timer and periodically sends a time signal message using
	/// an <see cref="IEventAggregator"/>. Any interested parties listen for the message on the same
	/// event aggregator.
	/// </summary>
	public class TimeSignalGenerator : TimeSignalGeneratorBase
	{
		private readonly IEventAggregator _eventAggregator;

		public TimeSignalGenerator(IEventAggregator eventAggregator, int period) : base(period)
 		{
			_eventAggregator = eventAggregator;
		}

		protected override void TimerCallback(object state)
		{
			if (_eventAggregator != null)
				_eventAggregator.SendMessage(new TimeSignal());
		}
	}
}
