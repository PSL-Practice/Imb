using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Imb.Data;

namespace Imb.Caching
{
    /// <summary>
    /// This class controls how many binaries are loaded into memory at any given time.
    /// 
    /// Users of a binary "claim" the binary, and while they hold the claim, it will not be unloaded.
    /// 
    /// However, unclaimed binaries are also kept in memory while there is spare capacity in the cache.
    /// When the cache fills, unlaimed binaries are unloaded in order to accomodate claimed binaries.
    /// The cache will cause a binary to be loaded when a claim requests it.
    /// 
    /// If the number of claimed binaries exceeds the configured maximum, the cache will run at over
    /// capacity until the claims are disposed. As they are disposed, the freed binaries will be unloaded
    /// until the number of binaries in the cache is reduced to the configured maximum.
    /// </summary>
    public class LoadedBinaryCache
    {
        private ClaimInterFace _claimInterface;

        public LoadedBinaryCache(int maxBinariesToLoad, IBinariesCache binaries)
        {
            _claimInterface = new ClaimInterFace(this, binaries, maxBinariesToLoad);
        }

        private interface IBinaryCache
        {
            void Release(Guid id);
        }

        public long NumItems { get { return _claimInterface.NumItems(); } }

        public IClaim GetBinary(Guid id)
        {
            return _claimInterface.Claim(id);
        }

        private class ClaimInterFace : IBinaryCache
        {
            private object _lock = new object();
            private readonly LoadedBinaryCache _cache;
            private readonly IBinariesCache _binaries;
            private readonly int _maxBinariesToLoad;
            public Dictionary<Guid, byte[]> BinaryData = new Dictionary<Guid, byte[]>(); 
            public Dictionary<Guid, int> Claims = new Dictionary<Guid, int>(); 

            public ClaimInterFace(LoadedBinaryCache cache, IBinariesCache binaries, int maxBinariesToLoad)
            {
                _cache = cache;
                _binaries = binaries;
                _maxBinariesToLoad = maxBinariesToLoad;
            }

            public BinaryClaim Claim(Guid id)
            {
                lock (_lock)
                {
                    byte[] binary;
                    if (!BinaryData.TryGetValue(id, out binary))
                    {
                        binary = _binaries.GetData(id);
                        BinaryData[id] = binary;
                    }

                    if (Claims.ContainsKey(id))
                        Claims[id]++;
                    else
                        Claims[id] = 1;

                    TrimCache();
                    return new BinaryClaim(id, this, binary);
                }
            }

            public void Release(Guid id)
            {
                lock (_lock)
                {
                    var count = Claims[id];
                    if (count == 1)
                    {
                        Claims.Remove(id);
                        TrimCache();
                    }
                    else
                        Claims[id]--;
                }
            }


            /// <summary>
            /// This method assumes a lock has been acquired
            /// </summary>
            private void TrimCache()
            {
                while (BinaryData.Count > _maxBinariesToLoad)
                {
                    var unclaimedId = BinaryData.Keys.Except(Claims.Keys).FirstOrDefault();
                    if (unclaimedId != Guid.Empty)
                        BinaryData.Remove(unclaimedId);
                    else
                        break;
                }
            }

            public long NumItems()
            {
                lock (_lock)
                    return BinaryData.Count;
            }
        }

        private class BinaryClaim : IClaim
        {
            private readonly Guid _id;
            private readonly IBinaryCache _cache;

            public BinaryClaim(Guid id, IBinaryCache cache, byte[] binary)
            {
                _id = id;
                _cache = cache;
                Binary = binary;
            }

            public void Dispose()
            {
                _cache.Release(_id);
                Binary = null;
            }

            public byte[] Binary { get; private set; }
        }
    }

    public interface IClaim : IDisposable
    {
        byte[] Binary { get; }
    }
}
