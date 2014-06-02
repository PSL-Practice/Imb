using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Imb.Data;
using Imb.Data.View;
using Imb.EventAggregation;

namespace TestImb.Mocks
{
    public class MockLibraryOperations : ILibraryOperations
    {
        private Dictionary<Guid, BinaryStorageClass> _binaries = new Dictionary<Guid, BinaryStorageClass>();
        public DateTime Date { get; set; }
        public List<string> History { get; set; }

        public Dictionary<int, string> Tags = new Dictionary<int, string>();
        public Dictionary<int, string[]> Paths = new Dictionary<int, string[]>();
        private UnitTestEventAggregator _eventAggregator;

        public MockLibraryOperations(UnitTestEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            History = new List<string>();
        }

        public IEnumerable<BinaryStorageClass> Binaries { get; private set; }
        public BinaryStorageClass AddFile(byte[] data, string name, string[] path, DateTime fileDateUtc, string fileContainerPath)
        {
            var id = Guid.NewGuid();
            History.Add(string.Format("Adding \"{0}\" Timestamp {1}, Size {2}, Container Path \"{3}\", Library path \"{4}\"",
                name, fileDateUtc, data.Length, fileContainerPath, path.Aggregate((t, i) => t + "\\" + i)));
            var binaryStorageClass = new BinaryStorageClass
            {
                DateAdded = Date == default(DateTime) ? DateTime.Now : Date,
                Id = id,
                NameTag = Tag(name),
                OriginalFileNameTag = Tag(name),
                OriginalPathTag = Tag(fileContainerPath),
                PathId = Path(path),
                OriginalBinaryDate = fileDateUtc,
            };

            _binaries.Add(id, binaryStorageClass);
            return binaryStorageClass;
        }

        private int Tag(string name)
        {
            var tagElement = Tags.FirstOrDefault(t => t.Value == name);
            if (tagElement.Value == null)
            {
                var next = Tags.Count() + 1;
                Tags[next] = name;
                return next;
            }

            return tagElement.Key;
        }

        private string Tag(int id)
        {
            string tag;
            if (!Tags.TryGetValue(id, out tag))
                return "??";

            return tag;
        }

        private int Path(string[] path)
        {
            var pathElement = Paths.FirstOrDefault(p => path.Length == p.Value.Length && StructuralComparisons.StructuralComparer.Compare(p.Value, path) == 0);
            if (pathElement.Value == null)
            {
                var next = Paths.Count() + 1;
                Paths[next] = path;
                return next;
            }

            return pathElement.Key;
        }

        private string[] Path(int id)
        {
            string[] path;
            if (!Paths.TryGetValue(id, out path))
                return new string[]{};

            return path;
        }

        public void DeleteFile(Guid id)
        {
            BinaryStorageClass item;
            if (!_binaries.TryGetValue(id, out item))
            {
                History.Add("Remove failed - Item not found");
                return;
            }

            History.Add(string.Format("Removing \"{0}\"", Tag(item.NameTag)));
            _binaries.Remove(id);
        }

        public void MoveFile(Guid id, string[] path, string name)
        {
            var existing = _binaries[id];
            var currentPath = PathString(Path(existing.PathId));
            var newPath = PathString(path);
            History.Add(string.Format("Moving \"{0}\" from \"{1}\" to \"{2}\", as \"{3}\"", Tag(existing.NameTag), currentPath, newPath, name));
            existing.PathId = Path(path);
        }

        private string PathString(string[] path)
        {
            return path.Aggregate((t, i) => t + "\\" + i);
        }

        public void Rename(Guid id, string newName)
        {
            var existing = _binaries[id];
            var currentName = Tag(existing.NameTag);
            existing.NameTag = Tag(newName);
            History.Add(string.Format("Renaming \"{0}\" to \"{1}\"", currentName, newName));
        }

        public string Print()
        {
            if (!History.Any()) return string.Empty;
            return History.Aggregate((t, i) => t + Environment.NewLine + i);
        }

        public LibraryFileNode MakeFileNode(BinaryStorageClass binary, Dispatcher dispatcher)
        {
            return new LibraryFileNode(dispatcher, _eventAggregator)
            {
                Name = Tags[binary.NameTag],
                Id = binary.Id,
                Path = Paths[binary.PathId],
            };
        }

        public BinaryStorageClass Get(string name)
        {
            var tag = Tag(name);
            var bin = _binaries.FirstOrDefault(b => b.Value.NameTag == tag);
            if (bin.Value != null)
                return bin.Value;

            return null;
        }

        public void WaitForItemChange(Guid id, Func<BinaryStorageClass, bool> changeDetected)
        {
            var sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < 500)
            {
                if (id != Guid.Empty)
                {
                    BinaryStorageClass item;
                    if (_binaries.TryGetValue(id, out item) && changeDetected(item))
                        break;
                }
                else
                {
                    if (changeDetected(null))
                        break;
                }
                Thread.Sleep(30);
            }
        }

    }

}