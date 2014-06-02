using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using ApprovalTests.Reporters;
using Imb.Data;
using Imb.Data.View;
using NUnit.Framework;
using TestImb.Mocks;
using UnitTestSupport;

namespace TestImb.Data
{
    [TestFixture]
    [UseReporter(typeof (CustomReporter))]
    public class TestLibraryOperations
    {
        private ILibraryOperations _ops;
        private byte[] _data;
        private DateTime _time;
        private AutoKillFolder _folder;
        private string _path;
        private Library _library;
        private MockFileValidator _fileValidator;


        [SetUp]
        public void SetUp()
        {
            _fileValidator = new MockFileValidator();
            _folder = new AutoKillFolder("TestLibrary", true);
            _path = Path.Combine(_folder.Path, "test");
            _library = new Library(_path, _fileValidator);
            _library.Create();
            _ops = _library.GetOperationsInterface();
            _data = new Byte[100];

            var random = new Random();
            random.NextBytes(_data);

            _time = DateTime.Parse("2014-01-04 11:10");
        }

        [TearDown]
        public void TearDown()
        {
            if (_library != null) _library.Dispose();
            _folder.Dispose();
        }

        [Test]
        public void FilesCanBeAdded()
        {
            var item = _ops.AddFile(_data, "test", new []{"new"}, _time, @"the\test\path");
            Reload();
            Assert.That(_ops.Binaries.Any(b => b.Id == item.Id));
        }

        [Test]
        public void FilesCanBeRenamed()
        {
            var item = _ops.AddFile(_data, "test", new []{"new"}, _time, @"the\test\path");
            var nameTag = item.NameTag;
            Reload();
            _ops.Rename(item.Id, "renamed");
            Reload();
            Assert.That(_ops.Binaries.First(b => b.Id == item.Id).NameTag, Is.GreaterThan(nameTag));
        }

        [Test, ExpectedException(typeof(NotFoundInLibraryException))]
        public void RenameWithMissingIdThrows()
        {
            _ops.Rename(Guid.NewGuid(), "renamed");
        }

        [Test]
        public void FilesCanBeDeleted()
        {
            var item = _ops.AddFile(_data, "test", new[] { "new" }, _time, @"the\test\path");
            Reload();
            var isPresent = _ops.Binaries.Any(b => b.Id == item.Id);
            _ops.DeleteFile(item.Id);
            Reload();
            var hasBeenDeleted = _ops.Binaries.All(b => b.Id != item.Id);

            var format = "Was present: {0}    Was deleted: {1}";
            var expected = string.Format(format, true, true);
            var actual = string.Format(format, isPresent, hasBeenDeleted);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test, ExpectedException(typeof(NotFoundInLibraryException))]
        public void DeleteWithMissingIdThrows()
        {
            _ops.DeleteFile(Guid.NewGuid());
        }

        [Test]
        public void MoveCanAlsoRename()
        {
            var item = _ops.AddFile(_data, "test", new[] { "new" }, _time, @"the\test\path");
            var nameTag = item.NameTag;
            Reload();
            _ops.MoveFile(item.Id, new []{"some", "new", "location"}, "renamed");
            Reload();
            Assert.That(_ops.Binaries.First(b => b.Id == item.Id).NameTag, Is.GreaterThan(nameTag));
        }

        [Test, ExpectedException(typeof(NotFoundInLibraryException))]
        public void MoveWithMissingIdThrows()
        {
            _ops.MoveFile(Guid.NewGuid(), new[] { "some", "new", "location" }, "renamed"); ;
        }

        private void Reload()
        {
            _library.Dispose();
            _library = new Library(_path, _fileValidator);
            _library.Open();
            _ops = _library.GetOperationsInterface();
        }
    }
}