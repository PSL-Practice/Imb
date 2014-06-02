using System;
using System.Threading;

namespace Imb.EventAggregation
{
	/// <summary>
	/// Simple periodic notification
	/// </summary>
	public class TimeSignal
	{
		/// <summary>
		/// Timestamp set when the instance was constructed. This is not the sent time.
		/// </summary>
		public DateTime Created { get; private set; }

		/// <summary>
		/// Unique index for this instance. No other instance will have this index. Each time a 
		/// <see cref="TimeSignal"/> is created, one is added to the index of the previous
		/// signal. The first signal created will have an index of 0.
		/// </summary>
		public long Index { get; private set; }
		
		private static long _globalIndex;


		public TimeSignal()
		{
			Created = DateTime.Now;
			Index = Interlocked.Increment(ref _globalIndex);
		}
	}
}
