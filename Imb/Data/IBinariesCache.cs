using System;
using System.Collections.Generic;

namespace Imb.Data
{
    public interface IBinariesCache
    {
        Guid Add(BinaryStorageClass details, byte[] data);
        BinaryStorageClass Get(Guid id);
        byte[] GetData(Guid id);
        void Delete(Guid id);
        void Update(BinaryStorageClass details);
        IEnumerable<BinaryStorageClass> Binaries { get; }
    }
}