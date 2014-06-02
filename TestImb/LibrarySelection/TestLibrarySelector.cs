using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Imb.Data;
using Imb.EventAggregation;
using Imb.LibrarySelection;
using NUnit.Framework;
using TestImb.Mocks;
using UnitTestSupport;

namespace TestImb.LibrarySelection
{
    [TestFixture]
    public class TestLibrarySelector
    {
        private AutoKillFolder _folder;
        private string _path;
        private string _path2;
        private UnitTestDispatcher _dispatcher;
        private LibrarySelector _selector;
        private MockFileValidator _fileValidator;
        private MockErrorHandler _errorHandler;
        private UnitTestEventAggregator _eventAggregator;

        [SetUp]
        public void SetUp()
        {
            _errorHandler = new MockErrorHandler();
            _fileValidator = new MockFileValidator();
            _dispatcher = new UnitTestDispatcher();
            _folder = new AutoKillFolder("TestLibrary", true);
            _path = Path.Combine(_folder.Path, "test");
            _path2 = Path.Combine(_folder.Path, "test2");
            _eventAggregator = new UnitTestEventAggregator();

            _selector = new LibrarySelector(_fileValidator, _errorHandler,  _dispatcher.Dispatcher, _eventAggregator);
        }

        [TearDown]
        public void TearDown()
        {
            if (_selector != null) _selector.Dispose();
        }

        [Test]
        public void LibrariesCanBeCreated()
        {
            var lib = _selector.CreateLibrary(_path);
            Assert.That(Directory.Exists(_path));
        }

        [Test]
        public void ViewIsCreatedForNewLibraries()
        {
            var lib = _selector.CreateLibrary(_path);
            Assert.That(lib, Is.Not.Null);
        }

        [Test, ExpectedException(typeof(IOException))]
        public void CreatedLibrariesAreHeldOpen()
        {
            var lib = _selector.CreateLibrary(_path);
            File.Delete(Path.Combine(_path, ".1"));
        }

        [Test]
        public void ViewIsCreatedForOpenedLibraries()
        {
            using (var newLib = new Library(_path, _fileValidator))
                newLib.Create();

            var lib = _selector.OpenLibrary(_path);
            Assert.That(lib, Is.Not.Null);
        }

        [Test, ExpectedException(typeof(IOException))]
        public void OpenedLibrariesAreHeldOpen()
        {
            using (var newLib = new Library(_path, _fileValidator))
                newLib.Create();

            var lib = _selector.OpenLibrary(_path);
            File.Delete(Path.Combine(_path, ".1"));
        }

        [Test]
        public void CurrentLibraryIsClosedOnClose()
        {
            using (var newLib = new Library(_path, _fileValidator))
                newLib.Create();

            var lib = _selector.OpenLibrary(_path);
            _selector.CloseLibrary(lib);
            File.Delete(Path.Combine(_path, ".1"));
        }

        [Test]
        public void CurrentLibraryIsClosedOnNewLibrary()
        {
            var lib = _selector.CreateLibrary(_path);
            var lib2 = _selector.CreateLibrary(_path2);
            File.Delete(Path.Combine(_path, ".1"));
        }

        [Test]
        public void CurrentLibraryIsClosedOnOpenLibrary()
        {
            using (var newLib = new Library(_path2, _fileValidator))
                newLib.Create();

            var lib = _selector.CreateLibrary(_path);
            var lib2 = _selector.OpenLibrary(_path2);
            File.Delete(Path.Combine(_path, ".1"));
        }
    }
}
