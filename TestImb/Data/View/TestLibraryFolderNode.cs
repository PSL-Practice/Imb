using Imb.Data.View;
using Imb.EventAggregation;
using NUnit.Framework;
using TestImb.Mocks;
using UnitTestSupport;

namespace TestImb.Data.View
{
    [TestFixture]
    public class TestLibraryFolderNode
    {
        private UnitTestDispatcher _unitTestDispatcher;
        private UnitTestEventAggregator _eventAggregator;
        private EventListener _listener;

        [SetUp]
        public void SetUp()
        {
            _eventAggregator = new UnitTestEventAggregator();
            _listener = new EventListener();
            _eventAggregator.AddListener(_listener);
            _unitTestDispatcher = new UnitTestDispatcher();
        }

        [Test]
        public void ByDefaultFolderIsNotInLabelEditMode()
        {
            var folder = new LibraryFolderNode(_unitTestDispatcher.Dispatcher, _eventAggregator);
            Assert.That(folder.EditLabel, Is.False);
        }

        [Test]
        public void EnteringInitialNameModeBeginsEdit()
        {
            var folder = new LibraryFolderNode(_unitTestDispatcher.Dispatcher, _eventAggregator);
            folder.EnterInitialNameMode();

            Assert.That(folder.EditLabel, Is.True);
        }

        [Test]
        public void InInitialNameModeCancelEditRequestsDelete()
        {
            var folder = new LibraryFolderNode(_unitTestDispatcher.Dispatcher, _eventAggregator)
            {
                Id = TestIds.GetId(0)
            };

            folder.EnterInitialNameMode();
            folder.CancelEditLabel.Execute(null);

            Assert.That(_listener.Requests, Is.EqualTo("Select Id0\r\nRemove Id0\r\n"));
        }

        [Test]
        public void OutsideInitialNameModeCancelEditDoesNotRequestDelete()
        {
            var folder = new LibraryFolderNode(_unitTestDispatcher.Dispatcher, _eventAggregator)
            {
                Id = TestIds.GetId(0)
            };

            folder.BeginEditLabel.Execute(null);
            folder.CancelEditLabel.Execute(null);

            Assert.That(_listener.Requests, Is.EqualTo("Select Id0\r\n"));
        }

        [Test]
        public void AfterInitialEditEndsCancelEditDoesNotRequestDelete()
        {
            var folder = new LibraryFolderNode(_unitTestDispatcher.Dispatcher, _eventAggregator)
            {
                Id = TestIds.GetId(0)
            };
            folder.EnterInitialNameMode();
            folder.FinishEditLabel.Execute(null);

            folder.BeginEditLabel.Execute(null);
            folder.CancelEditLabel.Execute(null);

            Assert.That(_listener.Requests, Is.EqualTo("Select Id0\r\nRename Id0 \r\nSelect Id0\r\n"));
        }

        [Test]
        public void NewFolderSpecifiesParent()
        {
            var folder = new LibraryFolderNode(_unitTestDispatcher.Dispatcher, _eventAggregator)
            {
                Id = TestIds.GetId(0),
                Path = new []{ "Parent"}
            };

            folder.NewFolder.Execute(null);

            Assert.That(_listener.Requests, Is.EqualTo("AddFolder Parent\\\r\n"));
        }
    }
}