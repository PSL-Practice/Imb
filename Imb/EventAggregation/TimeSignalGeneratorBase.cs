using System;
using System.Threading;

namespace Imb.EventAggregation
{
	/// <summary>
	/// This is the base class for time signal generators.
	/// </summary>
	public abstract class TimeSignalGeneratorBase : IDisposable
	{
		private readonly int _period;
		private readonly Timer _timer;

		protected TimeSignalGeneratorBase(int period)
		{
			_period = period;
			_timer = new Timer(TimerGuard, null, _period, 0);
		}

		private void TimerGuard(object state)
		{
			_timer.Change(0, Timeout.Infinite);
			TimerCallback(state);
			_timer.Change(_period, 0);
		}

		#region Implementation of IDisposable

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			_timer.Dispose();
			GC.SuppressFinalize(this);
		}

		#endregion

		protected abstract void TimerCallback(object state);
	}
}