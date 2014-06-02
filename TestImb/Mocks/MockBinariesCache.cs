using System;
using System.Collections.Generic;
using System.Linq;
using Imb.Data;

namespace TestImb.Mocks
{
    public class MockBinariesCache : IBinariesCache
    {
        private readonly MockBinaryDataCache _dataCache;
        private readonly Dictionary<Guid, BinaryStorageClass> _items = new Dictionary<Guid, BinaryStorageClass>();  

        public MockBinariesCache(MockBinaryDataCache dataCache)
        {
            _dataCache = dataCache;
        }

        public Guid Add(BinaryStorageClass details, byte[] data)
        {
            if (details.Id == Guid.Empty)
                details.Id = Guid.NewGuid();

            details.FirstDataBlockId = _dataCache.Write(data);
            _items[details.Id] = details;

            return Guid.Empty;
        }

        public BinaryStorageClass Get(Guid id)
        {
            return null;
        }

        public byte[] GetData(Guid id)
        {
            BinaryStorageClass item;
            if (_items.TryGetValue(id, out item))
                return _dataCache.Read(item.FirstDataBlockId);

            return null;
        }

        public void Delete(Guid id)
        {
        }

        public void Update(BinaryStorageClass details)
        {
        }

        public IEnumerable<BinaryStorageClass> Binaries { get { return _items.Values.ToList(); } }
    }
}