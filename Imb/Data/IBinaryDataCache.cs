using System;

namespace Imb.Data
{
    public interface IBinaryDataCache
    {
        Guid Write(byte[] data);
        void Commit();
        byte[] Read(Guid id);
        bool Exists(Guid id);
        void ReOpen();
    }
}