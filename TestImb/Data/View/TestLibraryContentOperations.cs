using System;
using System.Text;
using ApprovalTests.Reporters;
using Imb.Data.View;
using Imb.EventAggregation;
using Imb.Events;
using NUnit.Framework;
using TestImb.Mocks;
using UnitTestSupport;
using Approvals = ApprovalTests.Approvals;

namespace TestImb.Data.View
{
    [TestFixture]
    [UseReporter(typeof(CustomReporter))]
    public class TestLibraryContentOperations
    {
        private MockLibraryOperations _libOps;
        private LibraryContentOperations _ops;
        private MockLibraryView _libraryView;
        private byte[] _data;
        private UnitTestDispatcher _dispatcher;
        private MockFileValidator _fileValidator;
        private MockErrorHandler _errorHandler;
        private UnitTestEventAggregator _eventAggregator;

        [SetUp]
        public void SetUp()
        {
            _errorHandler = new MockErrorHandler();
            _fileValidator = new MockFileValidator();
            _dispatcher = new UnitTestDispatcher();
            _eventAggregator = new UnitTestEventAggregator();
            _libOps = new MockLibraryOperations(_eventAggregator);
            _libraryView = new MockLibraryView(_dispatcher.Dispatcher, _libOps, _errorHandler, _fileValidator, null, _eventAggregator);
            _ops = _libraryView.Operations;
            _data = new Byte[100];
        }

        [Test]
        public void AddedFilesAppearInRootOfLibrary()
        {
            _ops.AddFile(_data, "test", new [] { "root", "child"}, DateTime.MinValue, @"some\path");
            _dispatcher.RunDispatcher();
            var result = _libraryView.Print();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void AddedFilesAreSubmittedToLibrary()
        {
            _ops.AddFile(_data, "test", new [] { "root", "child"}, DateTime.MinValue, @"some\path");
            _dispatcher.RunDispatcher();
            var result = _libOps.Print();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void DeletedFilesAreRemovedFromLibrary()
        {
            _ops.AddFile(_data, "test", new [] { "root", "child"}, DateTime.MinValue, @"some\path");
            _ops.AddFile(_data, "test2", new [] { "root", "child"}, DateTime.MinValue, @"some\path");
            _dispatcher.RunDispatcher();

            var item = _libOps.Get("test");
            _ops.Delete(item.Id).Wait();
            _dispatcher.RunDispatcher();

            var result = _libOps.Print();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void DeletedFilesAreRemovedFromView()
        {
            _ops.AddFile(_data, "test", new [] { "root", "child"}, DateTime.MinValue, @"some\path");
            _ops.AddFile(_data, "test2", new [] { "root", "child"}, DateTime.MinValue, @"some\path");
            _dispatcher.RunDispatcher();

            var item = _libOps.Get("test");
            _ops.Delete(item.Id);
            _dispatcher.RunDispatcher();

            var result = _libraryView.Print();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void FoldersCanBeAddedToTheLibrary()
        {
            _ops.AddFolder(new[] { "root", "child" }, "test");
            _ops.AddFolder(new[] { "root", "child" }, "test2");
            _dispatcher.RunDispatcher();

            var result = _libraryView.Print();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void FilesCanBeRenamed()
        {
            _ops.AddFile(_data, "test", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _ops.AddFile(_data, "test2", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _dispatcher.RunDispatcher();

            var item = _libOps.Get("test");
            _ops.Rename(item.Id, "renamed").Wait();

            var result = PrintAll();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void FilesAreRenamedOnEventRequest()
        {
            _ops.AddFile(_data, "test", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _ops.AddFile(_data, "test2", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _dispatcher.RunDispatcher();

            var item = _libOps.Get("test");
            var currentTag = item.NameTag;
            _eventAggregator.SendMessage(new RenameRequest(item.Id, "renamed"));
            _libOps.WaitForItemChange(item.Id, i => i.NameTag != currentTag);

            var result = PrintAll();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void FilesAreRemovedOnEventRequest()
        {
            _ops.AddFile(_data, "test", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _ops.AddFile(_data, "test2", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _dispatcher.RunDispatcher();

            var item = _libOps.Get("test");
            _eventAggregator.SendMessage(new RemoveRequest(item.Id));
            _libOps.WaitForItemChange(Guid.Empty, i => _libOps.Get("test") != null);

            var result = PrintAll();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void FoldersCanBeRenamed()
        {
            _ops.AddFile(_data, "test", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _ops.AddFile(_data, "test2", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _dispatcher.RunDispatcher();

            var item = _libraryView.FindFirstNode("child");
            _ops.Rename(item.Id, "renamed").Wait();

            var result = PrintAll();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void FoldersCanBeDeleted()
        {
            _ops.AddFile(_data, "test", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _ops.AddFile(_data, "test2", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _dispatcher.RunDispatcher();

            var item = _libraryView.FindFirstNode("child");
            _ops.Delete(item.Id).Wait();

            var result = PrintAll();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void FoldersAreAddedAtTheRootOnRequest()
        {
            _ops.AddFile(_data, "test", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _ops.AddFile(_data, "test2", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _dispatcher.RunDispatcher();

            _eventAggregator.SendMessage(new AddNewFolder(null));

            var result = PrintAll();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void FoldersAreAddedAsChildOnRequest()
        {
            _ops.AddFile(_data, "test", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _ops.AddFile(_data, "test2", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _dispatcher.RunDispatcher();

            _eventAggregator.SendMessage(new AddNewFolder(new[] { "root", "child" }));
            _libOps.WaitForItemChange(Guid.Empty, i => _libOps.Get("") != null);

            var result = PrintAll();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void ItemsAreMovedOnRequest()
        {
            _ops.AddFile(_data, "test", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _ops.AddFile(_data, "test2", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _dispatcher.RunDispatcher();

            var node = _libraryView.FindFirstNode("test2");
            var parent = _libraryView.FindFirstNode("root");
            _eventAggregator.SendMessage(new MoveRequest(node, parent));
            _libOps.WaitForItemChange(Guid.Empty, i => parent.Children.Count > 1);

            var result = PrintAll();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void FoldersAreMovedOnRequest()
        {
            _ops.AddFile(_data, "test", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _ops.AddFile(_data, "test2", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _dispatcher.RunDispatcher();

            _eventAggregator.SendMessage(new AddNewFolder(new string[] {}));
            var node = _libraryView.FindFirstNode("child");
            var parent = _libraryView.FindFirstNode("New Folder");
            _eventAggregator.SendMessage(new MoveRequest(node, parent));
            _libOps.WaitForItemChange(Guid.Empty, i => parent.Children.Count > 1);

            var result = PrintAll();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void FoldersAreNotMovedIntoTheirOwnChildren()
        {
            _ops.AddFile(_data, "test", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _ops.AddFile(_data, "test2", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _dispatcher.RunDispatcher();

            _eventAggregator.SendMessage(new AddNewFolder(new[] { "root"}));
            var node = _libraryView.FindFirstNode("child");
            var parent = _libraryView.FindFirstNode("New Folder");
            _eventAggregator.SendMessage(new MoveRequest(node, parent));
            _libOps.WaitForItemChange(Guid.Empty, i => parent.Children.Count > 1);

            var result = PrintAll();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void CanBeMovedFromRootToChild()
        {
            _ops.AddFile(_data, "test", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _ops.AddFile(_data, "test2", new[] { "root", "child" }, DateTime.MinValue, @"some\path");
            _dispatcher.RunDispatcher();

            _eventAggregator.SendMessage(new AddNewFolder(new string[] {}));
            var node = _libraryView.FindFirstNode("root");
            var parent = _libraryView.FindFirstNode("New Folder");
            _eventAggregator.SendMessage(new MoveRequest(node, parent));
            _libOps.WaitForItemChange(Guid.Empty, i => parent.Children.Count > 1);

            var result = PrintAll();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        private string PrintAll()
        {
            var sb = new StringBuilder();
            sb.AppendLine("View");
            sb.AppendLine("~~~~");
            sb.AppendLine(_libraryView.Print());
            sb.AppendLine();
            sb.AppendLine("Library");
            sb.AppendLine("~~~~~~~");
            sb.AppendLine(_libOps.Print());
            return sb.ToString();
        }
    }
}
