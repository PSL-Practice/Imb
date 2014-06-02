using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Imb.Data;

namespace TestImb.Mocks
{
    public class MockPathsCache : IPathsCache
    {
        private readonly ITagCache _tags;
        private readonly Dictionary<int, int[]> _paths = new Dictionary<int, int[]>();

        public MockPathsCache(ITagCache tags)
        {
            _tags = tags;
        }

        public IEnumerable<int> AllPathIds { get { return _paths.Keys; }}

        public string[] GetPathStrings(int id)
        {
            int[] tags;
            if (!_paths.TryGetValue(id, out tags))
                return new string[] {};
            return tags.Select(t => _tags.Get(t)).ToArray();
        }

        public int AddOrGet(IEnumerable<int> tagsIn)
        {
            var tags = tagsIn.ToArray();
            var existing = _paths.FirstOrDefault(p => p.Value.Length == tags.Length && StructuralComparisons.StructuralComparer.Compare(p.Value, tags) == 0);
            if (existing.Value != null)
                return existing.Key;

            var id = _paths.Any() ? _paths.Keys.Max() + 1 : 1;
            _paths[id] = tags.ToArray();
            return id;
        }

        public int AddOrGet(IEnumerable<string> tags)
        {
            var path = tags.Select(t => _tags.AddOrGet(t)).ToArray();
            return AddOrGet(path);
        }

        public int[] Get(int id)
        {
            int[] path;
            if (_paths.TryGetValue(id, out path))
                return path;
            return null;
        }
    }
}