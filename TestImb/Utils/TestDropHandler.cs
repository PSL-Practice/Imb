using System;
using System.Collections.Generic;
using System.Windows;
using ApprovalTests;
using ApprovalTests.Reporters;
using Imb.EventAggregation;
using Imb.Utils;
using NUnit.Framework;
using TestImb.Mocks;
using UnitTestSupport;

namespace TestImb.Utils
{
    [TestFixture]
    [UseReporter(typeof(CustomReporter))]
    public class TestDropHandler
    {
        private MockLibraryView _view;
        private UnitTestDispatcher _dispatcher;
        private MockLibraryOperations _ops;
        private DropHandler _dropHandler;
        private MockErrorHandler _errorHandler;
        private MockFileValidator _fileValidator;
        private UnitTestEventAggregator _eventAggregator;

        [SetUp]
        public void SetUp()
        {
            _eventAggregator = new UnitTestEventAggregator();
            _dispatcher = new UnitTestDispatcher();
            _ops = new MockLibraryOperations(_eventAggregator);
            _errorHandler = new MockErrorHandler();
            _fileValidator = new MockFileValidator();
            _view = new MockLibraryView(_dispatcher.Dispatcher, _ops, _errorHandler, _fileValidator, "", _eventAggregator);
            _dropHandler = new DropHandler();
        }

        [Test, ExpectedException(typeof(InvalidDrop))]
        public void DropsWithNoViewOrErrorHandlerThrow()
        {
            _dropHandler.Drop(null, new DropArgs());
        }

        [Test]
        public void DropsWithNoViewLogAnError()
        {
            _dropHandler.SetErrorHandler(_errorHandler);
            _dropHandler.Drop(null, new DropArgs());
            Assert.That(_errorHandler.History, Is.EqualTo("Add failed\r\nDropping is invalid when no library is open.\r\n"));
        }

        [Test]
        public void DropsWithLibrarDoNotThrow()
        {
            _dropHandler.SetErrorHandler(_errorHandler);
            _dropHandler.SetLibrary(_view);
            _dropHandler.Drop(null, new DropArgs());
        }

        [Test]
        public void DropsWithAValidFileAddsANewBinary()
        {
            AttachServices();
            _dropHandler.Drop(null, new DropArgs { FileList = new List<string> { @"images\test.jpg" } });
            var output = _view.Print();
            Console.WriteLine(output);
            Approvals.Verify(output);
        }

        private void AttachServices()
        {
            _dropHandler.SetErrorHandler(_errorHandler);
            _dropHandler.SetLibrary(_view);
            _dropHandler.SetFileValidator(_fileValidator);
        }
    }
}
