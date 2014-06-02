using System;
using System.IO;
using System.Linq;
using System.Threading;
using Imb.Data;
using NUnit.Framework;
using TestImb.Mocks;
using UnitTestSupport;

namespace TestImb.Data
{
    [TestFixture]
    public class TestBinariesCache
    {
        private MockBinaryDataCache _data;
        private BinariesCache _cache;
        private byte[] _bytes;
        private AutoKillFolder _folder;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _folder = new AutoKillFolder("TestBinariesCache", true);
            Directory.CreateDirectory(_folder.Path);
            _path = Path.Combine(_folder.Path, "test");

            _data = new MockBinaryDataCache();
            _cache = new BinariesCache(_data);
            _cache.Create(_path);

            var random = new Random();
            _bytes = new byte[BinaryDataStorageClass.MaxBufferSize * 2];
            random.NextBytes(_bytes);
        }

        [TearDown]
        public void TearDown()
        {
            if (_cache != null)
            {
                _cache.Dispose();
                Thread.Sleep(50); //this seems to be needed when resharper is running the test suite, or else occasionally the test file is still open when the next test runs.
            }
            if (_folder != null) _folder.Dispose();
        }

        [Test]
        public void NewBinaryCanBeAdded()
        {
            var id = _cache.Add(new BinaryStorageClass(), _bytes);
            Assert.That(id, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void NewBinaryCanBeAddedAndTheDetailsRead()
        {
            var id = _cache.Add(new BinaryStorageClass(), _bytes);
            var details = _cache.Get(id);
            Assert.That(details.Id, Is.EqualTo(id));
        }

        [Test]
        public void BinaryCanBeDeleted()
        {
            var id = _cache.Add(new BinaryStorageClass(), _bytes);
            var details = _cache.Get(id);
            _cache.Delete(details.Id);
            Assert.That(_cache.Get(id), Is.Null);
        }

        [Test]
        public void FileIndexesAreIncremented()
        {
            var ids = Enumerable.Range(0, 20).Select(i => Guid.NewGuid()).ToList();
            foreach (var guid in ids)
            {
                _cache.Add(new BinaryStorageClass { Id = guid }, _bytes);
            }

            Assert.That(_cache.Get(ids.Last()).FileIndex, Is.EqualTo(20));
        }

        [Test]
        public void FileIndexIsRecoveredOnOpen()
        {
            var ids = Enumerable.Range(0, 20).Select(i => Guid.NewGuid()).ToList();
            foreach (var guid in ids)
            {
                _cache.Add(new BinaryStorageClass { Id = guid }, _bytes);
            }

            _cache.Dispose();
            _cache = new BinariesCache(_data);
            _cache.Open(_path);

            var newId = Guid.NewGuid();
            _cache.Add(new BinaryStorageClass { Id = newId}, _bytes);
            Assert.That(_cache.Get(newId).FileIndex, Is.EqualTo(21));
        }

        [Test]
        public void BinariesAreLoadedOnOpen()
        {
            var ids = Enumerable.Range(0, 20).Select(i => Guid.NewGuid()).ToList();
            foreach (var guid in ids)
            {
                _cache.Add(new BinaryStorageClass { Id = guid }, _bytes);
            }

            _cache.Dispose();
            _cache = new BinariesCache(_data);
            _cache.Open(_path);

            Assert.That(_cache.Binaries.Select(b => b.Id).OrderBy(b => b), Is.EqualTo(ids.OrderBy(i => i)));
        }

        [Test]
        public void BinariesCanBeUpdated()
        {
            var id = _cache.Add(new BinaryStorageClass(), _bytes);
            var details = _cache.Get(id);
            details.NameTag = 100;
            _cache.Update(details);

            _cache.Dispose();
            _cache = new BinariesCache(_data);
            _cache.Open(_path);

            Assert.That(_cache.Get(id).NameTag, Is.EqualTo(100));
        }

        [Test]
        public void DataCanBeRetrieved()
        {
            var id = _cache.Add(new BinaryStorageClass(), _bytes);
            var data = _cache.GetData(id);

            Assert.That(data, Is.EqualTo(_bytes));
        }

        [Test]
        public void BrokenSavesAreRecovered()
        {
            var ids = Enumerable.Range(0, 20).Select(i => Guid.NewGuid()).ToList();
            foreach (var guid in ids)
            {
                _cache.Add(new BinaryStorageClass { Id = guid }, _bytes);
            }

            var newGuid = Guid.NewGuid();
            _data.FailNextCommit();
            try
            {
                _cache.Add(new BinaryStorageClass { Id = newGuid }, _bytes);
            }
// ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }

            Assert.That(_cache.Get(newGuid), Is.Null);
        }

        [Test]
        public void DataCacheIsReopenedOnBrokenSave()
        {
            var ids = Enumerable.Range(0, 20).Select(i => Guid.NewGuid()).ToList();
            foreach (var guid in ids)
            {
                _cache.Add(new BinaryStorageClass { Id = guid }, _bytes);
            }

            var newGuid = Guid.NewGuid();
            _data.FailNextCommit();
            try
            {
                _cache.Add(new BinaryStorageClass { Id = newGuid }, _bytes);
            }
// ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }

            Assert.That(_data.Data.Keys.Count(k => _cache.Binaries.Any(b => b.FirstDataBlockId == k)), Is.EqualTo(_data.Data.Count()));
        }

        [Test]
        public void MissingDataIsRecoveredOnOpen()
        {
            var ids = Enumerable.Range(0, 20).Select(i => Guid.NewGuid()).ToList();
            foreach (var guid in ids)
            {
                _cache.Add(new BinaryStorageClass { Id = guid }, _bytes);
            }

            //simulate a failure in which saving the data failed to commit (which could be caused by power failure, for example).
            _data.Data.Remove(_cache.Get(ids.Last()).FirstDataBlockId);

            //Reopen the file, which should discard the last binary, because it's data is missing
            _cache.Dispose();
            _cache = new BinariesCache(_data);
            _cache.Open(_path);

            Assert.That(_cache.Get(ids.Last()), Is.Null);
        }
    }
}