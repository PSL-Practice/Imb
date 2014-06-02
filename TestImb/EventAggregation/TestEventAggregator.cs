using System;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Imb.EventAggregation;
using NUnit.Framework;

namespace TestImb.EventAggregation
{
	[TestFixture]
	public class TestEventAggregator
	{
		private readonly TestSync _testSync = new TestSync();
		private DispatcherFrame _testDispatcherFrame;

		class TestEvent
		{
			public string Text { get; set; }
		}

		class TestEvent2
		{
			public int Value { get; set; }
		}

		class TestEvent3
		{
		}

		[BatchedMessage]
		class BatchedEvent1
		{
			public static int Index = 0;

			public BatchedEvent1()
			{
				EventIndex = Index++;
			}

			public int EventIndex { get; private set; }
		}

		class TestSync : SynchronizationContext
		{
			public bool PostUsed { get; set; }

			public override void Post(SendOrPostCallback d, object state)
			{
				PostUsed = true;
				d.Invoke(state);	//This is not the correct symantics for Post, 
				//but it is called here so that we can prove
				//the callback is correct.
			}
		}

		class TestListener : IListener<TestEvent>, IListener<TestEvent2>, IListener<TestEvent3>, IListener<BatchedEvent1>
		{
			public string Result { get; private set; }

			public TestListener(IEventAggregator aggregator, SynchronizationContext synchronisationContext = null)
			{
				Result = string.Empty;
				if (synchronisationContext != null)
					aggregator.AddListener(this, synchronisationContext);
				else
					aggregator.AddListener(this);
			}

			public TestListener(IEventAggregator aggregator, Dispatcher dispatcher)
			{
				Result = string.Empty;
				if (dispatcher != null)
					aggregator.AddListener(this, dispatcher);
				else
					aggregator.AddListener(this);
			}

			#region Implementation of IListener<in TestEvent>

			/// <summary>
			/// Called when a message of the subscribed type is received.
			/// </summary>
			/// <param name="message"></param>
			public void Handle(TestEvent message)
			{
				Result += "TestEvent(" + message.Text + ")";
			}

			#endregion

			#region Implementation of IListener<in TestEvent2>

			/// <summary>
			/// Called when a message of the subscribed type is received.
			/// </summary>
			/// <param name="message"></param>
			public void Handle(TestEvent2 message)
			{
				Result += "TestEvent2(" + message.Value + ")";
			}

			#endregion

			#region Implementation of IListener<in TestEvent3>

			/// <summary>
			/// Called when a message of the subscribed type is received.
			/// </summary>
			/// <param name="message"></param>
			public void Handle(TestEvent3 message)
			{
				Result += "TestEvent3()";
			}

			#endregion

			public void Handle(BatchedEvent1 message)
			{
				Result += string.Format("BatchedEvent1({0})", message.EventIndex);
			}
		}

		[SetUp]
		public void SetUp()
		{
			_testDispatcherFrame = new DispatcherFrame();
			_testSync.PostUsed = false;
			BatchedEvent1.Index = 1;
		}

		[Test]
		public void TestClassHandlesTestEvent()
		{
			var agg = new EventAggregator();
			var tester = new TestListener(agg);
			agg.SendMessage(new TestEvent { Text = "test event" });

			Assert.That(tester.Result, Is.EqualTo("TestEvent(test event)"));
		}

		[Test]
		public void IfASynContextIsSuppliedItIsUsed()
		{
			var agg = new EventAggregator();
			var tester = new TestListener(agg, _testSync);
			agg.SendMessage(new TestEvent { Text = "test event" });

			Assert.That(_testSync.PostUsed, Is.True);
			Assert.That(tester.Result, Is.EqualTo("TestEvent(test event)"));
		}

		[Test]
		public void IfADispatcherIsSuppliedItIsUsed()
		{
			_testDispatcherFrame = new DispatcherFrame();
			var agg = new EventAggregator();
			var tester = new TestListener(agg, _testDispatcherFrame.Dispatcher);

			//this should queue a test event to the dispatcher
			agg.SendMessage(new TestEvent { Text = "test event" });

			//queue up a call that will end the dispatcher frame so that if something goes wrong we don't wait forever.
			_testDispatcherFrame.Dispatcher.BeginInvoke(new Action(() => _testDispatcherFrame.Continue = false));

			_testDispatcherFrame.Continue = true;
			Dispatcher.PushFrame(_testDispatcherFrame);

			Assert.That(tester.Result, Is.EqualTo("TestEvent(test event)"));
		}

		[Test]
		public void MessageInstancesAreSent()
		{
			var agg = new EventAggregator();
			var tester = new TestListener(agg);

			agg.SendMessage(new TestEvent { Text = "test event" });

			Assert.That(tester.Result, Is.EqualTo("TestEvent(test event)"));
		}

		[Test]
		public void DefaultMessageInstancesAreSent()
		{
			var agg = new EventAggregator();
			var tester = new TestListener(agg);

			agg.SendMessage<TestEvent3>();

			Assert.That(tester.Result, Is.EqualTo("TestEvent3()"));
		}

		[Test]
		public void ListenersAreRemoved()
		{
			var agg = new EventAggregator();
			var tester = new TestListener(agg, _testDispatcherFrame.Dispatcher);
			Assert.That(agg.GetMessageTypesHandled(tester).Any(), Is.True);

			agg.RemoveListener(tester);
			Assert.That(agg.GetMessageTypesHandled(tester).Any(), Is.False);
		}

		[Test]
		public void ListenersAreSubscribedToAllHandledMessages()
		{
			var agg = new EventAggregator();
			var tester = new TestListener(agg, _testDispatcherFrame.Dispatcher);
			var messageTypesHandled = agg.GetMessageTypesHandled(tester);
			Assert.That(messageTypesHandled.Count, Is.EqualTo(4));
			Assert.That(messageTypesHandled[0], Is.SameAs(typeof(TestEvent)));
			Assert.That(messageTypesHandled[1], Is.SameAs(typeof(TestEvent2)));
			Assert.That(messageTypesHandled[2], Is.SameAs(typeof(TestEvent3)));
			Assert.That(messageTypesHandled[3], Is.SameAs(typeof(BatchedEvent1)));

		}

		[Test]
		public void AllSubscribedListenersGetTheSentMessage()
		{
			var agg = new EventAggregator();
			var tester1 = new TestListener(agg);
			var tester2 = new TestListener(agg);
			var tester3 = new TestListener(agg);

			agg.SendMessage<TestEvent3>();

			Assert.That(tester1.Result, Is.EqualTo("TestEvent3()"));
			Assert.That(tester2.Result, Is.EqualTo("TestEvent3()"));
			Assert.That(tester3.Result, Is.EqualTo("TestEvent3()"));
		}

		[Test]
		public void UnsubscribedListenersStopGettingMessages()
		{
			var agg = new EventAggregator();
			var tester1 = new TestListener(agg);
			var tester2 = new TestListener(agg);
			var tester3 = new TestListener(agg);

			agg.SendMessage<TestEvent3>();

			Assert.That(tester1.Result, Is.EqualTo("TestEvent3()"));
			Assert.That(tester2.Result, Is.EqualTo("TestEvent3()"));
			Assert.That(tester3.Result, Is.EqualTo("TestEvent3()"));

			agg.RemoveListener(tester2);
			agg.SendMessage<TestEvent3>();

			Assert.That(tester1.Result, Is.EqualTo("TestEvent3()TestEvent3()"));
			Assert.That(tester2.Result, Is.EqualTo("TestEvent3()"));
			Assert.That(tester3.Result, Is.EqualTo("TestEvent3()TestEvent3()"));
		}

		[Test]
		public void BatchedMessagesAreBatched()
		{
			var agg = new EventAggregator();
			var tester = new TestListener(agg, _testDispatcherFrame.Dispatcher);
			_testDispatcherFrame.Continue = true;

			//this should queue a test event to the dispatcher
			for (var i = 0; i < 10; ++i) agg.SendMessage(new BatchedEvent1());

			//queue up a call that will end the dispatcher frame so that if something goes wrong we don't wait forever.
			_testDispatcherFrame.Dispatcher.BeginInvoke(new Action(() => _testDispatcherFrame.Continue = false));

			Dispatcher.PushFrame(_testDispatcherFrame);

			//Batching should delay the messages, so we expect nothing yet
			Assert.That(tester.Result, Is.EqualTo(""));
			Assert.That(agg.BatchesProcessed, Is.EqualTo(0), "Batches processed is not zero before batch test.");

			//Within 100ms the batch should be processed, so we allow 5 seconds for that to occur.
			var end = DateTime.Now + new TimeSpan(0, 0, 0, 5);
			do
			{
				_testDispatcherFrame.Continue = true;

				//queue up a call that will end the dispatcher frame so that if something goes wrong we don't wait forever.
				_testDispatcherFrame.Dispatcher.BeginInvoke(new Action(() => _testDispatcherFrame.Continue = false));

				Dispatcher.PushFrame(_testDispatcherFrame);
			} while (string.IsNullOrEmpty(tester.Result) && DateTime.Now <= end);

			Console.WriteLine(end - DateTime.Now);
			Console.WriteLine(agg.BatchesProcessed);
			Assert.That(tester.Result, Is.EqualTo("BatchedEvent1(1)BatchedEvent1(2)BatchedEvent1(3)BatchedEvent1(4)BatchedEvent1(5)BatchedEvent1(6)BatchedEvent1(7)BatchedEvent1(8)BatchedEvent1(9)BatchedEvent1(10)"));
			Assert.That(agg.BatchesProcessed, Is.EqualTo(1));
		}

	}
}
