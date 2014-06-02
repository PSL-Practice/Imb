using System;
using System.Windows.Threading;
using Imb.Data;
using Imb.Data.View;
using Imb.ErrorHandling;
using Imb.EventAggregation;
using Imb.Utils;

namespace Imb.LibrarySelection
{
    public class LibrarySelector : ILibrarySelector, IDisposable
    {
        private readonly object _lock = new object();
        private readonly IFileValidator _fileValidator;
        private readonly IErrorHandler _errorHandler;
        private readonly Dispatcher _dispatcher;
        private readonly IEventAggregator _eventAggregator;
        private Library _current;

        public LibrarySelector(IFileValidator fileValidator, IErrorHandler errorHandler, Dispatcher dispatcher, IEventAggregator eventAggregator)
        {
            _fileValidator = fileValidator;
            _errorHandler = errorHandler;
            _dispatcher = dispatcher;
            _eventAggregator = eventAggregator;
        }

        public ILibraryView CreateLibrary(string path)
        {
            lock (_lock)
            {
                var lib = new Library(path, _fileValidator);
                lib.Create();
                SetCurrent(lib);
                return MakeViewForCurrent(lib);
                
            }
        }

        public ILibraryView OpenLibrary(string path)
        {
            lock (_lock)
            {
                var lib = new Library(path, _fileValidator);
                lib.Open();
                SetCurrent(lib);
                return MakeViewForCurrent(lib);
            }
        }

        public void CloseLibrary(ILibraryView library)
        {
            lock (_lock)
            {
                if (_current != null)
                {
                    _current.Dispose();
                    _current = null;
                }
            }
        }

        private void SetCurrent(Library lib)
        {
            if (_current != null)
                _current.Dispose();
            _current = lib;
        }

        private ILibraryView MakeViewForCurrent(Library lib)
        {
            lock (_lock)
            {
                var internalAccess = lib as ILibrary;
                return new LibraryView(internalAccess.TagCache, internalAccess.PathsCache, internalAccess.BinariessCache,
                    _dispatcher, lib.GetOperationsInterface(), _errorHandler, _fileValidator, _eventAggregator);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_current != null)
                    _current.Dispose();
            }
        }
    }
}
