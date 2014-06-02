using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Data.FileTypes;

namespace Imb.Data
{
    public class BinariesCache : IDisposable, IBinariesCache
    {
        private readonly object _lock = new object();
        private readonly IBinaryDataCache _data;
        private FixedFile<BinaryStorageClass> _file;
        private Dictionary<Guid, BinaryStorageClass> _binaries = new Dictionary<Guid, BinaryStorageClass>();
        private int _nextIndex;
        private string _path;

        public BinariesCache(IBinaryDataCache data)
        {
            _data = data;
        }

        public IEnumerable<BinaryStorageClass> Binaries { get { return _binaries.Values.ToList(); } }

        public void Create(string path)
        {
            lock (_lock)
            {
                _path = path;
                _file = FixedFile<BinaryStorageClass>.Create(path, 1);
                _nextIndex = 1;
            }
        }

        public void Open(string path)
        {
            lock (_lock)
            {
                _path = path;
                _file = FixedFile<BinaryStorageClass>.Open(path);

                _binaries = _file.GetRecords().ToDictionary(r => r.Id);

                PerformRecovery();
                _nextIndex = FindMaxFileIndex() + 1;
            }
        }

        private void PerformRecovery()
        {
            bool recovered;
            do
            {
                recovered = false;
                var maxFileIndex = FindMaxFileIndex();
                var lastItem = _binaries.FirstOrDefault(b => b.Value.FileIndex == maxFileIndex);
                if (lastItem.Value != null)
                {
                    var dataId = lastItem.Value.FirstDataBlockId;
                    if (!_data.Exists(dataId))
                    {
                        recovered = true;
                        Delete(lastItem.Value.Id);
                    }
                }
            } while (recovered);
        }

        private int FindMaxFileIndex()
        {
            return _binaries.Any() ? _binaries.Max(b => b.Value.FileIndex) : 0;
        }

        public void Dispose()
        {
            lock (_lock)
                if (_file != null)
                {
//                    Console.WriteLine("Spon");
                    _file.Dispose();
                }
        }

        /// <summary>
        /// Add a binary to the file and store the data in the data cache.
        /// 
        /// It should be noted that there is a failure mode in which the commit of the binary details file could work, but the commit 
        /// of the data could fail. This would result in the record of the binary being stored, but no actual data would be present
        /// in the files. This would manifest itself as an exception, which is caught here, and an attempt to remove the binary details
        /// is made. In the event that the data commit fails because of catastrophic failure or power loss, the recovery attempt may not
        /// take place or may itself fail. This would leave the data in an inconsistent state, but only for the most recently saved binary.
        /// 
        /// On open, the most recently saved binary is checked, to see if the data is present. If it is not, it is removed at that stage.
        /// On the assumption that the failure to 
        /// </summary>
        /// <param name="details"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Guid Add(BinaryStorageClass details, byte[] data)
        {
            lock (_lock)
            {
                try
                {
                    if (details.Id == Guid.Empty)
                        details.Id = Guid.NewGuid();
                    details.FileIndex = _nextIndex++;
                    details.FirstDataBlockId = _data.Write(data);
                    _file.Write(details);
                    _file.Commit();

                    _binaries.Add(details.Id, details);

                    //Make sure that the file is removed if commit fails.
                    Action reverseOutFile = () =>
                    {
                        // ReSharper disable AccessToDisposedClosure
                        //Note: Resharper cannot determine whether the lambda will outlive the try/catch block. It can't so we can ignore the warning.
                        _file.Delete(details);
                        _file.Commit();
                        // ReSharper restore AccessToDisposedClosure
                    };

                    using (var reverse = new OnError(reverseOutFile))
                    {
                        _data.Commit();
                        reverse.Commit(); //if we reach here, the commit worked and we don't need to reverse out the file.
                    }

                    return details.Id;
                }
                catch
                {
                    _file.Dispose();
                    _file = null;
                    Open(_path);
                    _data.ReOpen();
                    throw;
                }
            }
        }

        public BinaryStorageClass Get(Guid id)
        {
            lock (_lock)
            {
                BinaryStorageClass details;
                if (_binaries.TryGetValue(id, out details))
                    return details;

                return null;
                
            }
        }

        public byte[] GetData(Guid id)
        {
            lock (_lock)
            {
                var details = Get(id);
                if (details == null) return null;

                return _data.Read(details.FirstDataBlockId);
            }
        }

        public void Delete(Guid id)
        {
            lock (_lock)
            {
                _file.Delete(new BinaryStorageClass {Id = id});
                _file.Commit();
                _binaries.Remove(id);
                
            }
        }

        public void Update(BinaryStorageClass details)
        {
            lock (_lock)
            {
                _file.Write(details);
                _file.Commit();
            }
        }
    }
}