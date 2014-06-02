using System;
using System.Collections.Generic;
using System.Linq;
using ApprovalTests.Reporters;
using Imb.Caching;
using Imb.Data;
using NUnit.Framework;
using TestImb.Mocks;
using UnitTestSupport;
using Data.Utilities;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed
namespace TestImb.Caching
{
    [TestFixture]
    [UseReporter(typeof (CustomReporter))]
    public class TestLoadedBinariesCache
    {
        private MockBinaryDataCache _dataCache;
        private MockBinariesCache _binaries;
        private byte[] _bytes;
        private LoadedBinaryCache _cache;
        private List<BinaryStorageClass> _items;
        private const int CacheCapacity = 5;

        [SetUp]
        public void SetUp()
        {
            _dataCache = new MockBinaryDataCache();
            _binaries = new MockBinariesCache(_dataCache);

            var random = new Random();
            _bytes = new byte[BinaryDataStorageClass.MaxBufferSize * 2];
            random.NextBytes(_bytes);

            _items = Enumerable.Range(0,40).Select(i => new BinaryStorageClass { Id = Guid.NewGuid()}).ToList();

            foreach (var bsc in _items)
            {
                _binaries.Add(bsc, _bytes);
            }

            _cache = new LoadedBinaryCache(CacheCapacity, _binaries);
        }

        [Test]
        public void BinariesAreLoadedWhenClaimed()
        {
            var id = _items[0].Id;
            using (var claim = _cache.GetBinary(id))
                Assert.That(claim.Binary, Is.SameAs(_bytes));
        }

        [Test]
        public void LoadedBinaryIsHeldInCache()
        {
            var id = _items[0].Id;
            using (_cache.GetBinary(id))
            {
                
            }

            Assert.That(_cache.NumItems, Is.EqualTo(1));
        }

        [Test]
        public void CacheBecomesOversizedIfClaimsExceedCapacity()
        {
            using (var mg = new MultiGuard())
            {
                var claims = _items.Take(10).Select(i => mg.Accept(_cache.GetBinary(i.Id))).ToList();
                Assert.That(_cache.NumItems, Is.EqualTo(claims.Count));
            }
        }

        [Test]
        public void ExcessItemsAreRemovedFromCacheWhenClaimsAreReleased()
        {
            using (var mg = new MultiGuard())
            {
                _items.Take(10).Select(i => mg.Accept(_cache.GetBinary(i.Id))).ToList();
            }
            Assert.That(_cache.NumItems, Is.EqualTo(CacheCapacity));
        }

        [Test]
        public void UnclaimedItemsAreDiscardedToMakeRoomForNewClaims()
        {
            using (var mg = new MultiGuard())
            {
                _items.Take(CacheCapacity).Select(i => mg.Accept(_cache.GetBinary(i.Id))).ToList();
            }

            //cache is now full of unclaimed items. In order to load the last one, an old one needs to be discarded.
            using (_cache.GetBinary(_items.Last().Id))
                Assert.That(_cache.NumItems, Is.EqualTo(CacheCapacity));
        }

        [Test]
        public void TwoClaimsCreateAClaimCountOfTwo()
        {
            using (var mg = new MultiGuard())
            {
                _items.Take(CacheCapacity).Select(i => mg.Accept(_cache.GetBinary(i.Id))).ToList();
                var guid = _items.Last().Id;
                using (_cache.GetBinary(guid))
                {
                    //cache is now oversize - claim and release the same item again. the cache should stay oversized
                    using (_cache.GetBinary(guid))
                    {
                    }
                    Assert.That(_cache.NumItems, Is.EqualTo(CacheCapacity + 1));
                }
            }
        }

        [Test]
        public void ItemsAreReleasedWhenAllClaimsAreDisposed()
        {
            using (var mg = new MultiGuard())
            {
                _items.Take(CacheCapacity).Select(i => mg.Accept(_cache.GetBinary(i.Id))).ToList();
                var guid = _items.Last().Id;
                using (_cache.GetBinary(guid))
                {
                    //cache is now oversize - claim and release the same item again. the cache should stay oversized
                    using (_cache.GetBinary(guid))
                    {
                    }
                }

                //cache should now be full (there are still enough claims to keep it full, so the only one released will be the double claimed one)
                Assert.That(_cache.NumItems, Is.EqualTo(CacheCapacity));
            }
        }
    }
}
