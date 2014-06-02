using System;
using Data.SchemaHandling;

namespace Imb.Data
{
    public class BinaryDataStorageClass
    {
        public static int MaxBufferSize = 10000;

        [SchemaKey] public Guid Id;

        [FixedArray(10000)]
        public byte[] Bytes;

        public Guid NextBlock;
        public int RemainingBytes;
    }
}