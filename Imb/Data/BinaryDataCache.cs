using System;
using System.Collections.Generic;
using System.Windows.Documents;
using Data.FileTypes;

namespace Imb.Data
{
    public class BinaryDataCache : IDisposable, IBinaryDataCache
    {
        private FixedFile<BinaryDataStorageClass> _file;
        private string _path;

        public void Create(string path)
        {
            _path = path;
            _file = FixedFile<BinaryDataStorageClass>.Create(path, 1);
        }

        public void Open(string path)
        {
            _path = path;
            _file = FixedFile<BinaryDataStorageClass>.Open(path);
        }

        public void ReOpen()
        {
            _file.Dispose();
            Open(_path);
        }

        public void Dispose()
        {
            if (_file != null) _file.Dispose();
        }

        public void Commit()
        {
            _file.Commit();
        }

        public Guid Write(byte[] data)
        {
            var offset = 0;
            byte[] buffer = null;
            var id = Guid.Empty;
            while (offset < data.Length)
            {
                var nextBlockSize = Math.Min(data.Length - offset, BinaryDataStorageClass.MaxBufferSize);
                if (buffer == null || buffer.Length != nextBlockSize)
                    buffer = new byte[nextBlockSize];
                Array.Copy(data, offset, buffer, 0, nextBlockSize);
                var nextBlock = new BinaryDataStorageClass
                {
                    Bytes = buffer,
                    Id = Guid.NewGuid(),
                    NextBlock = id,
                    RemainingBytes = offset + nextBlockSize
                };
                _file.Write(nextBlock);
                offset += nextBlockSize;
                id = nextBlock.Id;
            }

            return id;
        }

        public byte[] Read(Guid id)
        {
            byte[] buffer = null;
            do
            {
                var block = new BinaryDataStorageClass {Id = id};
                if (!_file.Get(block))
                    return null;

                if (buffer == null)
                    buffer = new byte[block.RemainingBytes];

                Array.Copy(block.Bytes, 0, buffer, block.RemainingBytes - block.Bytes.Length, block.Bytes.Length);
                id = block.NextBlock;
            }
            while (id != Guid.Empty) ;

            return buffer;
        }

        public bool Exists(Guid id)
        {
            var block = new BinaryDataStorageClass { Id = id };
            return _file.Get(block);
        }

        public void Delete(Guid id)
        {
            var ids = new List<Guid>();
            var nextId = id;

            var block = new BinaryDataStorageClass();

            do
            {
                block.Id = nextId;
                if (!_file.Get(block))
                    break;

                ids.Add(nextId);

                nextId = block.NextBlock;
            } while (block.NextBlock != null);

            ids.Reverse();
            foreach (var deleteId in ids)
            {
                block.Id = deleteId;
                _file.Delete(block);
            }
        }
    }
}