using System;
using System.IO;
using System.Linq;
using ApprovalTests;
using ApprovalTests.Reporters;
using Imb.Data;
using NUnit.Framework;
using TestImb.Mocks;
using UnitTestSupport;

namespace TestImb.Data
{
    [TestFixture]
    [UseReporter(typeof (CustomReporter))]
    public class TestLibrary
    {
        private AutoKillFolder _folder;
        private string _path;
        private UnitTestDispatcher _dispatcher;
        private MockFileValidator _fileValidator;

        [SetUp]
        public void SetUp()
        {
            _fileValidator = new MockFileValidator();
            _dispatcher = new UnitTestDispatcher();
            _folder = new AutoKillFolder("TestLibrary", true);
            _path = Path.Combine(_folder.Path, "test");
        }

        [TearDown]
        public void TearDown()
        {
            _folder.Dispose();
        }


        [Test]
        public void LibraryCanBeInitialised()
        {
            using (var lib = new Library(_path, _fileValidator))
                lib.Create();
            Assert.That(Directory.Exists(_path));
        }

        [Test]
        public void DataFilesAreCreatedByCreateCall()
        {
            using (var lib = new Library(_path, _fileValidator))
                lib.Create();

            var result = Directory.GetFiles(_path).Aggregate((t, i) => t + Environment.NewLine + i);
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void ExistingLibraryCanBeOpened()
        {
            using (var lib = new Library(_path, _fileValidator))
                lib.Create();

            using (var lib = new Library(_path, _fileValidator))
                lib.Open();
        }

        [Test, ExpectedException(typeof(RootNotFoundException))]
        public void OpenThrowsIfLibraryIsMissing()
        {
            using (var lib = new Library(Path.Combine(_path, "X"), _fileValidator))
                lib.Open();
        }

        [Test, ExpectedException(typeof(RootNotDirectoryException))]
        public void OpenThrowsIfLibraryIsNotAFolderMissing()
        {
            Directory.CreateDirectory(_path);
            var root = Path.Combine(_path, "X");
            File.WriteAllBytes(root, new byte[] {0, 1, 2});

            using (var lib = new Library(root, _fileValidator))
                lib.Open();
        }

        [Test, ExpectedException(typeof (RootAlreadyExistsException))]
        public void ExceptionThrownByCreateIfFolderAlreadyPresent()
        {
            Directory.CreateDirectory(_path);
            var lib = new Library(_path, _fileValidator);
            lib.Create();
        }
    }
}
