using System;
using System.IO;
using Imb.Data;
using NUnit.Framework;
using UnitTestSupport;

namespace TestImb.Data
{
    [TestFixture]
    public class TestBinaryDataCache
    {
        private AutoKillFolder _folder;
        private string _path;
        private BinaryDataCache _cache;
        private byte[] _data;
        private byte[] _oneBlockData;
        private byte[] _tinyData;

        [SetUp]
        public void SetUp()
        {
            _folder = new AutoKillFolder("TestBinaryDataCache", true);
            Directory.CreateDirectory(_folder.Path);
            _path = Path.Combine(_folder.Path, "test");

            _cache = new BinaryDataCache();
            _cache.Create(_path);

            var random = new Random();
            _data = new byte[(int)(BinaryDataStorageClass.MaxBufferSize * 4.5)];
            random.NextBytes(_data);

            _oneBlockData = new byte[BinaryDataStorageClass.MaxBufferSize];
            Array.Copy(_data, _oneBlockData, _oneBlockData.Length);

            _tinyData = new byte[1];
            Array.Copy(_data, _tinyData, _tinyData.Length);
        }

        [TearDown]
        public void TearDown()
        {
            if (_cache != null) _cache.Dispose();
            if (_folder != null) _folder.Dispose();
        }

        [Test]
        public void DataCanBeWrittenAndRetrieved()
        {
            var id = _cache.Write(_data);
            _cache.Commit();
            var loaded = _cache.Read(id);
            Assert.That(loaded, Is.EqualTo(_data));
        }

        [Test]
        public void SingleBlockDataCanBeWrittenAndRetrieved()
        {
            var id = _cache.Write(_oneBlockData);
            _cache.Commit();
            var loaded = _cache.Read(id);
            Assert.That(loaded, Is.EqualTo(_oneBlockData));
        }

        [Test]
        public void TinyBlockDataCanBeWrittenAndRetrieved()
        {
            var id = _cache.Write(_tinyData);
            _cache.Commit();
            var loaded = _cache.Read(id);
            Assert.That(loaded, Is.EqualTo(_tinyData));
        }

        [Test]
        public void FetchingMissingDataReturnsNull()
        {
            var id = _cache.Write(_tinyData);
            _cache.Commit();
            var loaded = _cache.Read(Guid.NewGuid());
            Assert.That(loaded, Is.Null);
        }

        [Test]
        public void CheckingForExistingItemReturnsTrue()
        {
            var id = _cache.Write(_tinyData);
            _cache.Commit();
            Assert.That(_cache.Exists(id), Is.True);
        }

        [Test]
        public void CheckingForNonExistantItemReturnsFalse()
        {
            var id = _cache.Write(_tinyData);
            _cache.Commit();
            Assert.That(_cache.Exists(Guid.NewGuid()), Is.False);
        }

        [Test]
        public void BinaryDataCanBeDeleted()
        {
            var id = _cache.Write(_data);
            _cache.Commit();

            _cache.Delete(id);
            _cache.Commit();
            Assert.That(_cache.Exists(id), Is.False);
        }
    }
}