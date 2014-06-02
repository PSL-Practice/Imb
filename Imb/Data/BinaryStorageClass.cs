using System;
using Data.SchemaHandling;

namespace Imb.Data
{
    public class BinaryStorageClass
    {
        [SchemaKey] public Guid Id;
        public Guid FirstDataBlockId;

        public int OriginalFileNameTag;
        public int OriginalPathTag;
        public int FileIndex;
        public int NameTag;
        public int PathId;
        public DateTime DateAdded;
        public DateTime OriginalBinaryDate;
    }
}