using System.Windows;
using Data.RecordStreamImpl;
using Imb.Data.View;
using Imb.EventAggregation;
using Imb.Utils;
using NUnit.Framework;
using TestImb.Mocks;
using UnitTestSupport;

namespace TestImb.Data.View
{
    [TestFixture]
    public class TestLibraryViewNode
    {
        private UnitTestDispatcher _unitTestDispatcher;
        private LibraryFileNode _node;
        private UnitTestEventAggregator _eventAggregator;
        private EventListener _listener;
        private LibraryFolderNode _folderNode;

        [SetUp]
        public void SetUp()
        {
            _eventAggregator = new UnitTestEventAggregator();
            _listener = new EventListener();
            _eventAggregator.AddListener(_listener);
            _unitTestDispatcher = new UnitTestDispatcher();
            _node = new LibraryFileNode(_unitTestDispatcher.Dispatcher, _eventAggregator);
            _node.Id = TestIds.GetId(0);
            _node.Id = TestIds.GetId(0);
            _node.Name = "original name";

            _folderNode = new LibraryFolderNode(_unitTestDispatcher.Dispatcher, _eventAggregator)
            {
                Id = TestIds.GetId(1),
                Path=new []{"Parent"}
            };
        }

        [Test]
        public void StartEditSetsEditNameToCurrentName()
        {
            _node.BeginEditLabel.Execute(null);

            Assert.That(_node.EditName, Is.EqualTo(_node.Name));
        }

        [Test]
        public void EditNameIsNotSetWhenNoNameEditingIsInProgress()
        {
            Assert.That(_node.EditName, Is.Not.EqualTo(_node.Name));
        }

        [Test]
        public void WhenEditingFinishEditSavesEditValue()
        {
            _node.BeginEditLabel.Execute(null);
            _node.EditName = "Cheese";
            _node.FinishEditLabel.Execute(null);

            Assert.That(_node.Name, Is.EqualTo("Cheese"));
        }

        [Test]
        public void WhenEditingCancelEditSavesEditValue()
        {
            _node.BeginEditLabel.Execute(null);
            _node.EditName = "Cheese";
            _node.CancelEditLabel.Execute(null);

            Assert.That(_node.Name, Is.EqualTo("original name"));
        }

        [Test]
        public void WhenEditingCancelEditDoesNotRequestRename()
        {
            _node.BeginEditLabel.Execute(null);
            _node.EditName = "Cheese";
            _node.CancelEditLabel.Execute(null);

            Assert.That(_listener.Requests, Is.EqualTo("Select Id0\r\n"));
        }

        [Test]
        public void WhenEditingFinishEditRequestsRename()
        {
            _node.BeginEditLabel.Execute(null);
            _node.EditName = "Cheese";
            _node.FinishEditLabel.Execute(null);

            Assert.That(_listener.Requests, Is.EqualTo("Select Id0\r\nRename Id0 Cheese\r\n"));
        }

        [Test]
        public void RemoveNodeRequestsDelete()
        {
            _node.RemoveNode.Execute(null);

            Assert.That(_listener.Requests, Is.EqualTo("Remove Id0\r\n"));
        }

        [Test]
        public void NewFolderRequestsNewFolder()
        {
            _folderNode.NewFolder.Execute(null);

            Assert.That(_listener.Requests, Is.EqualTo("AddFolder Parent\\\r\n"));
        }

        [Test]
        public void DropRequestsMove()
        {
            _folderNode.Drop(_node);

            Assert.That(_listener.Requests, Is.EqualTo("Move LibraryFileNode {original name} to LibraryFolderNode {}\r\n"));
        }

    }
}