namespace Imb.EventAggregation
{
	/// <summary>
	/// This class runs a timer and periodically calls the EventAggregator via a private interface
	/// so that it can schedule batched message execution.
	/// </summary>
	internal class AggregatorTimeSignalGenerator : TimeSignalGeneratorBase
	{
		private readonly IPrivateEventAggregator _eventAggregator;

		public AggregatorTimeSignalGenerator(IPrivateEventAggregator eventAggregator, int period) : base(period)
		{
			_eventAggregator = eventAggregator;
		}

		protected override void TimerCallback(object state)
		{
			if (_eventAggregator != null)
				_eventAggregator.TimeSignalFired();
		}

	}
}