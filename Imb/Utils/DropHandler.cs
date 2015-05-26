using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Windows;
using System.Windows.Documents;
using Imb.Data;
using Imb.Data.View;
using Imb.DropHandling.DataHandlers;
using Imb.ErrorHandling;

namespace Imb.Utils
{
    public interface IDataHandler
    {
        bool TryGetData(IDataObject dataObject, out DropArgs output);
    }

    public class DropHandler
    {
        public static DropHandler Current { get { return _current; } }
        private static DropHandler _current = new DropHandler();

        public static IEnumerable<IDataHandler> DataHandlers {get { return _dataHandlers; }}

        private ILibraryView _library;
        private IErrorHandler _errorHandler;
        private IFileValidator _fileValidator;
        private static List<IDataHandler> _dataHandlers = null;

        public DropHandler()
        {
            
        }

        public void SetErrorHandler(IErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        public void SetLibrary(ILibraryView libraryView)
        {
            _library = libraryView;
        }

        public void Drop(object container, DropArgs dropArgs)
        {
            if (_errorHandler == null)
                throw new InvalidDrop("No library open or error handler set.");

            if (_library == null)
            {
                _errorHandler.LogError("Add failed", "Dropping is invalid when no library is open.");
                return;
            }

            if (dropArgs.Data != null)
            {
                _library.Operations.AddFile(dropArgs.Data, Path.GetFileName(dropArgs.OriginalPath), null, DateTime.Now, dropArgs.OriginalPath);
            }
            else if (dropArgs.FileList != null)
            {
                foreach (var filePath in dropArgs.FileList)
                {
                    _library.Operations.AddFile(filePath);
                }
                
            }
        }

        public void SetFileValidator(IFileValidator fileValidator)
        {
            _fileValidator = fileValidator;
        }

        public static void RegisterDataHandlers()
        {
            _dataHandlers = new List<IDataHandler>
            {
                new GoogleImagesDropHandler(),
                new URLDropDataHandler(),
                new FileDropDataHandler(),
            };
        }
    }

    public class InvalidDrop : Exception
    {
        public InvalidDrop(string message) : base(message)
        {
        }
    }
}
