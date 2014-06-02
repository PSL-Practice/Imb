using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Data.FileTypes;

namespace Imb.Data
{
    public class TagsCache : IDisposable, ITagCache
    {
        private readonly object _lock = new object();
        private RecordStream<int> _file;
        private Dictionary<string, int> _tags = new Dictionary<string, int>();
        private Dictionary<int, string> _strings = new Dictionary<int, string>();
        private int _nextTagId;

        public IEnumerable<string> Tags
        {
            get { lock (_lock) return _tags.Keys.ToList(); }
        }

        public void Create(string path)
        {
            _file = new RecordStream<int>();
            _file.Create(path);
            DeclareRecordType();
        }

        public void Dispose()
        {
            if (_file != null) _file.Dispose();
        }

        public int AddOrGet(string tag)
        {
            int key;
            lock (_lock)
                if (!_tags.TryGetValue(tag, out key))
                {
                    var id = NextTagId();
                    _tags.Add(tag, key = id);
                    _strings.Add(id, tag);
                    _file.Write(new TagStorageClass { Tag = tag});
                    _file.Flush();
                }

            return key;
        }

        public string Get(int tag)
        {
            string value;
            return _strings.TryGetValue(tag, out value) ? value : "??";
        }

        private int NextTagId()
        {
            return Interlocked.Increment(ref _nextTagId);
        }

        public void Open(string path)
        {
            _file = new RecordStream<int>();
            _file.Open(path);
            DeclareRecordType();

            LoadTags();
        }

        private void LoadTags()
        {
            foreach (var tag in _file.ReadRecords<TagStorageClass>())
            {
                var id = NextTagId();
                _tags[tag.Tag] = id;
                _strings[id] = tag.Tag;
            }
        }

        private void DeclareRecordType()
        {
            _file.DeclareRecordType(0, typeof (TagStorageClass));
        }
    }

    public interface ITagCache
    {
        int AddOrGet(string tag);
        string Get(int id);
    }
}