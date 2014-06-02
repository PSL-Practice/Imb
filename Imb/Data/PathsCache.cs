using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data.FileTypes;
using Data.RecordHandling;

namespace Imb.Data
{
    public class PathsCache : IDisposable, IPathsCache
    {
        private readonly ITagCache _tagCache;
        private readonly object _lock = new object(); 
        private RecordStream<int> _file;

        private readonly Dictionary<int[], int> _paths = new Dictionary<int[], int>();
        private readonly Dictionary<int, int[]> _pathLookup = new Dictionary<int, int[]>();
        private int _nextPathId;
        public IEnumerable<int[]> Paths { get { lock (_lock) return _paths.Keys.ToList(); } }

        public PathsCache(ITagCache tagCache)
        {
            _tagCache = tagCache;
        }

        public void Create(string path)
        {
            _file = new RecordStream<int>();
            _file.Create(path);
            DeclareRecordType();
        }

        private void DeclareRecordType()
        {
            _file.DeclareRecordType(0, typeof (PathStorageClass));
        }

        public void Dispose()
        {
            if (_file != null) _file.Dispose();
        }

        public int AddOrGet(IEnumerable<int> tagsIn)
        {
            var tags = tagsIn.ToArray();
            lock (_lock)
            {
                int id;
                var item = _paths.FirstOrDefault(p => StructuralComparisons.StructuralEqualityComparer.Equals(p.Key, tags));
                if (item.Key == null)
                {
                    id = GetNextPathId();
                    _paths[tags] = id;
                    _pathLookup[id] = tags;
                    _file.Write(new PathStorageClass { Tags = tags });
                    _file.Flush();
                    return id;
                }
                return item.Value;
            }
        }

        private int GetNextPathId()
        {
            return ++_nextPathId;
        }

        public int AddOrGet(IEnumerable<string> tags)
        {
            var ids = tags.Select(t => _tagCache.AddOrGet(t));
            return AddOrGet(ids);
        }

        public void Open(string path)
        {
            lock (_lock)
            {
                _file = new RecordStream<int>();
                DeclareRecordType();
                _file.Open(path);

                LoadPaths();
            }
        }

        private void LoadPaths()
        {
            foreach (var path in _file.ReadRecords<PathStorageClass>())
            {
                var id = GetNextPathId();
                _paths[path.Tags] = id;
                _pathLookup[id] = path.Tags;
            }
        }

        public int[] Get(int id)
        {
            int[] path;
            if (_pathLookup.TryGetValue(id, out path))
                return path.ToArray();
            return null;
        }
    }
}