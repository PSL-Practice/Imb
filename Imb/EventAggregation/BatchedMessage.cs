using System;
using System.Threading;
using System.Windows.Threading;

namespace Imb.EventAggregation
{
	/// <summary>
	/// This class is an attribute that can be attached to messages causing them to be batched by the event aggregator.
	/// Messages tagged in this way will be batched automatically and the caller does not need to take any action.
	/// However, batching messages changes their semantics dramatically, in that *unbatched* messages are dispatched
	/// immediately, and in some cases, the Send is effectively a method call on the listeners (where there is no
	/// <see cref="Dispatcher"/> or <see cref="SynchronizationContext"/>). Once the message is batched, however, the
	/// message will probably not be handled immediately. Although the batched messages will still be handled in the 
	/// same order as they were generated, the execution is no longer deterministic and any hidden dependencies on 
	/// when the fact that messages are processed as they are sent will be broken. 
	/// </summary>
	public class BatchedMessage : Attribute
	{
	}
}
