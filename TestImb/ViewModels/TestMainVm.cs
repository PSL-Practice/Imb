using System;
using ApprovalTests;
using ApprovalTests.Reporters;
using Imb;
using Imb.EventAggregation;
using Imb.ViewModels;
using NUnit.Framework;
using TestImb.Mocks;
using UnitTestSupport;

namespace TestImb.ViewModels
{
    [TestFixture]
    [UseReporter(typeof (CustomReporter))]
    public class TestMainVm
    {
        private MainVm _vm;
        private MockLibraryLocationDialog _locationDialog;
        private MockLibrarySelector _librarySelector;
        private UnitTestDispatcher _testDispatcher;
        private MockErrorHandlerView _errorHandlerView;
        private ImbSettings _settings;
        private MockFileValidator _fileValidator;
        private UnitTestEventAggregator _eventAggregator;
        private EventListener _listener;

        [SetUp]
        public void SetUp()
        {
            _eventAggregator = new UnitTestEventAggregator();
            _fileValidator = new MockFileValidator();
            _testDispatcher = new UnitTestDispatcher();
            _locationDialog = new MockLibraryLocationDialog();
            _errorHandlerView = new MockErrorHandlerView();
            _librarySelector = new MockLibrarySelector(_testDispatcher.Dispatcher, null, _errorHandlerView.MockErrorHandler, _fileValidator, _eventAggregator);
            _settings = new ImbSettings();
            _listener = new EventListener();
            _eventAggregator.AddListener(_listener);
            _vm = new MainVm(_locationDialog, _librarySelector, _errorHandlerView, new MockFileValidator(), _settings, _eventAggregator);
        }

        [Test]
        public void NewCommandPromptsForLibraryLocation()
        {
            _locationDialog.PathToReturn = "path";
            _vm.NewCommand.Execute(null);
            var expected = "Requested new library location." + Environment.NewLine;
            Assert.That(_locationDialog.History, Is.EqualTo(expected));
        }

        [Test]
        public void NewCommandDoesNotCreateLibraryWhenLocationIsNull()
        {
            _locationDialog.PathToReturn = null;
            _vm.NewCommand.Execute(null);
            var expected = string.Empty;
            Assert.That(_librarySelector.History, Is.EqualTo(expected));
        }

        [Test]
        public void NewCommandCreatesLibrary()
        {
            _locationDialog.PathToReturn = "path";
            _vm.NewCommand.Execute(null);
            var expected = "Create library in location \"path\"." + Environment.NewLine;
            Assert.That(_librarySelector.History, Is.EqualTo(expected));
        }

        [Test]
        public void NewCommandSavesPathToSettings()
        {
            _locationDialog.PathToReturn = "path";
            _vm.NewCommand.Execute(null);
            Assert.That(_settings.LibraryPath, Is.EqualTo(_locationDialog.PathToReturn));
        }

        [Test]
        public void OpenCommandPromptsForLibraryLocation()
        {
            _locationDialog.PathToReturn = "path";
            _vm.OpenCommand.Execute(null);
            var expected = "Requested existing library location." + Environment.NewLine;
            Assert.That(_locationDialog.History, Is.EqualTo(expected));
        }

        [Test]
        public void OpenCommandDoesNotOpenLibraryWhenLocationIsNull()
        {
            _locationDialog.PathToReturn = null;
            _vm.OpenCommand.Execute(null);
            var expected = string.Empty;
            Assert.That(_librarySelector.History, Is.EqualTo(expected));
        }

        [Test]
        public void OpenCommandOpensLibrary()
        {
            _locationDialog.PathToReturn = "path";
            _vm.OpenCommand.Execute(null);
            var expected = "Open library in location \"path\"." + Environment.NewLine;
            Assert.That(_librarySelector.History, Is.EqualTo(expected));
        }

        [Test]
        public void OpenCommandSavesPathToSettings()
        {
            _locationDialog.PathToReturn = "path";
            _vm.OpenCommand.Execute(null);
            Assert.That(_settings.LibraryPath, Is.EqualTo(_locationDialog.PathToReturn));
        }

        [Test]
        public void SecondOpenClosesFirstLibrary()
        {
            _locationDialog.PathToReturn = "path";
            _vm.OpenCommand.Execute(null);
            _locationDialog.PathToReturn = "path2";
            _vm.OpenCommand.Execute(null);
            
            var expected = "Open library in location \"path\"." + Environment.NewLine;
            expected += "Open library in location \"path2\"." + Environment.NewLine;
            expected += "Close library in location \"path\"." + Environment.NewLine;

            Console.WriteLine(_librarySelector.History);
            Assert.That(_librarySelector.History, Is.EqualTo(expected));
        }

        [Test]
        public void SecondNewClosesFirstLibrary()
        {
            _locationDialog.PathToReturn = "path";
            _vm.NewCommand.Execute(null);
            _locationDialog.PathToReturn = "path2";
            _vm.NewCommand.Execute(null);
            
            var expected = "Create library in location \"path\"." + Environment.NewLine;
            expected += "Create library in location \"path2\"." + Environment.NewLine;
            expected += "Close library in location \"path\"." + Environment.NewLine;

            Console.WriteLine(_librarySelector.History);
            Assert.That(_librarySelector.History, Is.EqualTo(expected));
        }

        [Test]
        public void ByDefaultAddIsDisabled()
        {
            Assert.That(_vm.AddCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void AddIsEnabledByNew()
        {
            _locationDialog.PathToReturn = "path";
            _vm.NewCommand.Execute(null);
            Assert.That(_vm.AddCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void AddIsEnabledByOpen()
        {
            _locationDialog.PathToReturn = "path";
            _vm.OpenCommand.Execute(null);
            Assert.That(_vm.AddCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void AddPromptsForFileName()
        {
            _locationDialog.PathToReturn = "path\\file";
            _vm.NewCommand.Execute(null);
            _vm.AddCommand.Execute(null);
            var expected = "Requested new library location." + Environment.NewLine;
            expected += "Requested file location." + Environment.NewLine;
            Assert.That(_locationDialog.History, Is.EqualTo(expected));
        }

        [Test]
        public void AddCreatesNewFilesFolder()
        {
            _locationDialog.PathToReturn = @"images\bad.jpg";
            _vm.NewCommand.Execute(null);
            _vm.AddCommand.Execute(null);

            var view = _vm.Library as MockLibraryView;
            var result = view.Print();
            Approvals.Verify(result);
        }

        [Test]
        public void NewFolderCommandUnavailableByDefault()
        {
            Assert.That(_vm.NewFolderCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void NewFolderCommandIsEnabledByNew()
        {
            _locationDialog.PathToReturn = "path";
            _vm.NewCommand.Execute(null);
            Assert.That(_vm.NewFolderCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void NewFolderCommandIsEnabledByOpen()
        {
            _locationDialog.PathToReturn = "path";
            _vm.OpenCommand.Execute(null);
            Assert.That(_vm.NewFolderCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void NewFolderCommandMakesNewFolderRequest()
        {
            _locationDialog.PathToReturn = "path";
            _vm.OpenCommand.Execute(null);
            _vm.NewFolderCommand.Execute(null);
            Assert.That(_listener.Requests, Is.EqualTo("AddFolder \r\nSelect missing\r\n"));
        }
    }
}
