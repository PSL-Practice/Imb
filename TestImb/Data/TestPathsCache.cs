using System;
using System.IO;
using System.Linq;
using System.Text;
using ApprovalTests;
using ApprovalTests.Reporters;
using Imb.Data;
using NUnit.Framework;
using TestImb.Mocks;
using UnitTestSupport;

namespace TestImb.Data
{
    [TestFixture]
#if USEWINMERGE
    [UseReporter(typeof(WinMergeReporter))]
#else
    [UseReporter(typeof(QuietReporter))]
#endif
    public class TestPathsCache
    {
        private AutoKillFolder _folder;
        private string _path;
        private PathsCache _cache;
        private MockTagsCache _tags;

        [SetUp]
        public void SetUp()
        {
            _folder = new AutoKillFolder("TestPathsCache", true);
            Directory.CreateDirectory(_folder.Path);
            _path = Path.Combine(_folder.Path, "test");

            _tags = new MockTagsCache();
            _cache = new PathsCache(_tags);
            _cache.Create(_path);

            _tags.AddOrGet("A");

        }

        [TearDown]
        public void TearDown()
        {
            if (_cache != null) _cache.Dispose();
            if (_folder != null) _folder.Dispose();
        }

        [Test]
        public void BrandNewCacheIsEmpty()
        {
            Assert.That(_cache.Paths.Count(), Is.EqualTo(0));
        }

        [Test]
        public void AddingAPathCreatesANewPath()
        {
            _cache.AddOrGet(new []{"test", "path"});
            Assert.That(_cache.Paths.Count(), Is.EqualTo(1));
        }

        [Test]
        public void AddingAnExistingPathReturnsItsIndex()
        {
            var strings = new[] { "test", "path" };
            _cache.AddOrGet(strings);
            Assert.That(_cache.AddOrGet(strings), Is.EqualTo(1));
        }

        [Test]
        public void PathsAreAllocatedIncreasingIndexes()
        {
            var tags = new[]
            {
                "Z", "Y", "X", "W", "V", "U", "T"
            };

            for (int ix = 0; ix < tags.Length; ix++)
            {
                _cache.AddOrGet(tags.Take(ix + 1));
            }

            var result = LoadPaths();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void PathsAreLoadedOnOpen()
        {
            var tags = new[]
            {
                "alpha", "beta", "charlie", "delta", "echo", "foxtrot", "golf"
            };
            
            for (int ix = 0; ix < tags.Length; ix++)
            {
                _cache.AddOrGet(tags.Take(ix + 1));
            }

            var opened = new PathsCache(_tags);
            _cache.Dispose();

            _cache = opened;
            opened.Open(_path);

            var result = LoadPaths(opened);
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void PathsCanBeRetrievedByKeyAfterOpen()
        {
            var tags = new[]
            {
                "alpha", "beta", "charlie", "delta", "echo", "foxtrot", "golf"
            };

            var id = 0;
            for (var ix = 0; ix < tags.Length; ix++)
            {
                id = _cache.AddOrGet(tags.Take(ix + 1));
            }

            var opened = new PathsCache(_tags);
            _cache.Dispose();

            _cache = opened;
            opened.Open(_path);

            var path = _cache.Get(id);
            Assert.That(path, Is.EqualTo(new [] {2, 3, 4, 5, 6, 7, 8}));
        }

        [Test]
        public void AddingTheFirstTagCreatesIdOne()
        {
            Assert.That(_cache.AddOrGet(new[] { "test", "path" }), Is.EqualTo(1));
        }

        [Test]
        public void PathsCanBeRetrieved()
        {
            var id = _cache.AddOrGet(new[] { "test", "path" });
            var path = _cache.Get(id);

            string result = path.Select(t => _tags.Get(t)).Aggregate((t, i) => t + ", " + i);
            Assert.That(result, Is.EqualTo("test, path"));
        }

        [Test]
        public void GetOnMissingPathReturnsNull()
        {
            Assert.That(_cache.Get(100), Is.Null);
        }

        private string LoadPaths(PathsCache cache = null)
        {
            if (cache == null)
                cache = _cache;

            var sb = new StringBuilder();
            foreach (var item in cache.Paths)
            {
                var itemTags = item
                    .Select(i => _tags.Tags.FirstOrDefault(c => c.Value == i).Key ?? "NULL")
                    .Aggregate((t, i) => t + "," + i);
                sb.AppendFormat("{0} = {1} [{2}]", item, _cache.AddOrGet(item), itemTags);
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}   