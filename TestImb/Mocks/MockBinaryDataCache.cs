using System;
using System.Collections.Generic;
using Imb.Data;

namespace TestImb.Mocks
{
    public class MockBinaryDataCache : IBinaryDataCache
    {
        public readonly Dictionary<Guid, byte[]> Data = new Dictionary<Guid, byte[]>();
        private readonly List<Guid> _uncommitted = new List<Guid>();
        private bool _failCommit;

        public Guid Write(byte[] data)
        {
            var id = Guid.NewGuid();
            Data[id] = data;
            _uncommitted.Add(id);
            return id;
        }

        public void Commit()
        {
            if (_failCommit)
            {
                _failCommit = false;
                throw new Exception();
            }
            _uncommitted.Clear();
        }

        public byte[] Read(Guid id)
        {
            byte[] data;
            if (!Data.TryGetValue(id, out data))
                return null;

            return data;
        }

        public bool Exists(Guid id)
        {
            return Data.ContainsKey(id);
        }

        public void ReOpen()
        {
            foreach (var guid in _uncommitted)
            {
                Data.Remove(guid);
            }
        }

        public void FailNextCommit()
        {
            _failCommit = true;
        }
    }
}