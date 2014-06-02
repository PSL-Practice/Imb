using System;
using System.IO;
using System.Linq;
using System.Text;
using ApprovalTests;
using ApprovalTests.Reporters;
using Data.FileTypes;
using Imb.Data;
using NUnit.Framework;
using UnitTestSupport;

namespace TestImb.Data
{
    [TestFixture]
    [UseReporter(typeof(CustomReporter))]
    public class TestTagsCache
    {
        private AutoKillFolder _folder;
        private string _path;
        private TagsCache _cache;

        [SetUp]
        public void SetUp()
        {
            _folder = new AutoKillFolder("TestTagsCache", true);
            Directory.CreateDirectory(_folder.Path);
            _path = Path.Combine(_folder.Path, "test");
            _cache = new TagsCache();
            _cache.Create(_path);
        }

        [TearDown]
        public void TearDown()
        {
            if (_cache != null) _cache.Dispose();
            _folder.Dispose();
        }


        [Test]
        public void BrandNewCacheIsEmpty()
        {
            Assert.That(_cache.Tags.Count(), Is.EqualTo(0));
        }

        [Test]
        public void AddingATagCreatesANewTag()
        {
            _cache.AddOrGet("test");
            Assert.That(_cache.Tags.Count(), Is.EqualTo(1));
        }

        [Test]
        public void AddingAnExistingTagReturnsItsIndex()
        {
            _cache.AddOrGet("test");
            Assert.That(_cache.AddOrGet("test"), Is.EqualTo(1));
        }

        [Test]
        public void TagsAreAllocatedIncreasingIndexes()
        {
            var tags = new[]
            {
                "Z", "Y", "X", "W", "V", "U", "T"
            };

            foreach (var tag in tags)
                _cache.AddOrGet(tag);

            var result = LoadTags();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void TagsAreLoadedOnOpen()
        {
            var tags = new[]
            {
                "alpha", "beta", "charlie", "delta", "echo", "foxtrot", "golf"
            };

            foreach (var tag in tags)
                _cache.AddOrGet(tag);

            var opened = new TagsCache();
            _cache.Dispose();

            _cache = opened;
            opened.Open(_path);

            var result = LoadTags(opened);
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void TagsCanBeRetrievedByKeyAfterOpen()
        {
            var tags = new[]
            {
                "alpha", "beta", "charlie", "delta", "echo", "foxtrot", "golf"
            };

            var id = 0;
            foreach (var tag in tags)
                id = _cache.AddOrGet(tag);

            var opened = new TagsCache();
            _cache.Dispose();

            _cache = opened;
            opened.Open(_path);

            var result = _cache.Get(id);
            Assert.That(result, Is.EqualTo("golf"));
        }

        [Test]
        public void TagsAreNotReWrittenOnOpen()
        {
            var tags = new[]
            {
                "alpha", "beta", "charlie", "delta", "echo", "foxtrot", "golf"
            };

            foreach (var tag in tags)
                _cache.AddOrGet(tag);
            _cache.Dispose();
            _cache = null;

            using (var opened = new TagsCache())
            {
                opened.Open(_path);
                opened.Dispose();
            }

            using (var file = new RecordStream<int>())
            {
                file.Open(_path);
                file.DeclareRecordType(0, typeof(TagStorageClass));

                Assert.That(file.ReadRecords<TagStorageClass>().Count(t => t.Tag == "charlie"), Is.EqualTo(1));
            }
        }

        [Test]
        public void AddingTheFirstTagCreatesIdOne()
        {
            Assert.That(_cache.AddOrGet("test"), Is.EqualTo(1));
        }

        [Test]
        public void TagsCanBeFetchedById()
        {
            var id = _cache.AddOrGet("test");
            Assert.That(_cache.Get(id), Is.EqualTo("test"));
        }

        [Test]
        public void GetOnMissingTagReturnsFiller()
        {
            Assert.That(_cache.Get(100), Is.EqualTo("??"));
        }

        private string LoadTags(TagsCache cache = null)
        {
            if (cache == null)
                cache = _cache;

            var sb = new StringBuilder();
            foreach (var tag in cache.Tags)
            {
                sb.AppendFormat("{0} = {1}", tag, _cache.AddOrGet(tag));
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}