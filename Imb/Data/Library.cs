using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Imb.Annotations;
using Imb.Utils;

namespace Imb.Data
{
    public class Library : IDisposable, INotifyPropertyChanged, ILibrary
    {
        private readonly IFileValidator _fileValidator;
        private readonly string _root;
        private TagsCache _tags;
        private string _binariesPath;
        private string _binaryDataPath;
        private string _tagsPath;
        private string _pathsPath;
        private PathsCache _paths;
        private BinariesCache _binaries;
        private BinaryDataCache _binaryData;
        private LibraryOperations _operations;

        public Library(string root, IFileValidator fileValidator)
        {
            _fileValidator = fileValidator;
            _root = Path.GetFullPath(root);
            _binariesPath = Path.Combine(_root, ".1");
            _binaryDataPath = Path.Combine(_root, ".2");
            _tagsPath = Path.Combine(_root, ".3");
            _pathsPath = Path.Combine(_root, ".4");
        }

        public void Create()
        {
            if (Directory.Exists(_root) 
                && Directory.EnumerateFileSystemEntries(_root).Any())
            {
                throw new RootNotEmptyException(_root);
            }

            if (!Directory.Exists(_root))
                Directory.CreateDirectory(_root);

            try
            {
                InitialiseNewLibrary();
            }
            catch
            {
                Safe(() => Directory.Delete(_root, true));
                throw;
            }
            _operations = new LibraryOperations(_tags, _paths, _binaries, _binaryData, _fileValidator);
        }

        public void Open()
        {
            if (File.Exists(_root))
                throw new RootNotDirectoryException(_root);
            
            if (!Directory.Exists(_root))
                throw new RootNotFoundException(_root);

            InitialiseExistingLibrary();
            _operations = new LibraryOperations(_tags, _paths, _binaries, _binaryData, _fileValidator);
        }

        private void Safe(Action action)
        {
            try
            {
                action();
            }
// ReSharper disable once EmptyGeneralCatchClause
            catch{}
        }

        private void InitialiseNewLibrary()
        {
            try
            {
                CreateTagsFile(true);
                CreatePathsFile(true);
                CreateBinaryDataFile(true);
                CreateBinariesFile(true);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        private void InitialiseExistingLibrary()
        {
            CreateTagsFile();
            CreatePathsFile();
            CreateBinaryDataFile();
            CreateBinariesFile();
        }

        private void CreateTagsFile(bool create = false)
        {
            _tags = new TagsCache();
            if (create)
                _tags.Create(_tagsPath);
            else
                _tags.Open(_tagsPath);
        }

        private void CreatePathsFile(bool create = false)
        {
            _paths = new PathsCache(_tags);
            if (create)
                _paths.Create(_pathsPath);
            else
                _paths.Open(_pathsPath);
        }

        private void CreateBinariesFile(bool create = false)
        {
            _binaries = new BinariesCache(_binaryData);
            if (create)
                _binaries.Create(_binariesPath);
            else
                _binaries.Open(_binariesPath);
        }

        private void CreateBinaryDataFile(bool create = false)
        {
            _binaryData = new BinaryDataCache();
            if (create)
                _binaryData.Create(_binaryDataPath);
            else
                _binaryData.Open(_binaryDataPath);
        }

        public void Dispose()
        {
            Action<IDisposable> disposer = d =>
            {
                if (d != null) d.Dispose();
            };

            disposer(_binaries);
            disposer(_binaryData);
            disposer(_paths);
            disposer(_tags);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private class LibraryOperations : ILibraryOperations
        {
            private readonly ITagCache _tagCache;
            private readonly IPathsCache _pathsCache;
            private readonly IBinariesCache _binariesCache;
            private readonly IBinaryDataCache _dataCache;
            private readonly IFileValidator _fileValidator;

            public LibraryOperations(ITagCache tagCache, IPathsCache pathsCache, IBinariesCache binariesCache, IBinaryDataCache dataCache, IFileValidator fileValidator)
            {
                _tagCache = tagCache;
                _pathsCache = pathsCache;
                _binariesCache = binariesCache;
                _dataCache = dataCache;
                _fileValidator = fileValidator;
            }

            public BinaryStorageClass AddFile(byte[] data, string name, string[] path, DateTime fileDateUtc, string fileContainerPath)
            {
                var nameTag = _tagCache.AddOrGet(name);
                var originalPathTag = _tagCache.AddOrGet(fileContainerPath);
                var pathId = _pathsCache.AddOrGet(path);

                var binary = new BinaryStorageClass
                {
                    Id = Guid.NewGuid(),
                    DateAdded = DateTime.Now.ToUniversalTime(),
                    OriginalBinaryDate = fileDateUtc,
                    OriginalFileNameTag = nameTag,
                    OriginalPathTag = originalPathTag,
                    NameTag = nameTag,
                    PathId = pathId,
                };

                _binariesCache.Add(binary, data);

                return binary;
            }

            public void DeleteFile(Guid id)
            {
                var binary = _binariesCache.Get(id);
                if (binary == null)
                    throw new NotFoundInLibraryException(id);

                _binariesCache.Delete(id);
            }

            public void MoveFile(Guid id, string[] path, string name)
            {
                var binary = _binariesCache.Get(id);
                if (binary == null)
                    throw new NotFoundInLibraryException(id);

                var tagId = _tagCache.AddOrGet(name);
                var pathId = _pathsCache.AddOrGet(path);
                binary.NameTag = tagId;
                binary.PathId = pathId;
                _binariesCache.Update(binary);
            }

            public void Rename(Guid id, string newName)
            {
                var binary = _binariesCache.Get(id);
                if (binary == null)
                    throw new NotFoundInLibraryException(id);

                var tag = _tagCache.AddOrGet(newName);
                binary.NameTag = tag;
                _binariesCache.Update(binary);
            }

            public IEnumerable<BinaryStorageClass> Binaries { get { return _binariesCache.Binaries; } } 
        }

        public ILibraryOperations GetOperationsInterface()
        {
            return _operations;
        }

        ITagCache ILibrary.TagCache
        {
            get { return _tags; }
        }

        IPathsCache ILibrary.PathsCache
        {
            get { return _paths; }
        }

        IBinariesCache ILibrary.BinariessCache
        {
            get { return _binaries; }
        }
    }
}
