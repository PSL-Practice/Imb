using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Imb.Annotations;
using Imb.Caching;
using Imb.Data.View;
using Imb.ErrorHandling;
using Imb.EventAggregation;
using Imb.Events;
using Imb.LibrarySelection;
using Imb.UI;
using Imb.Utils;
using Utils.UISupport;

namespace Imb.ViewModels
{
    public sealed class MainVm : INotifyPropertyChanged
    {
        private readonly ILibraryLocationDialog _locationDialog;
        private readonly ILibrarySelector _librarySelector;
        public ICommand NewCommand { get { return _newCommand; } }
        public ICommand OpenCommand { get { return _openCommand; } }
        public ICommand AddCommand { get { return _addCommand; } }
        public ICommand NewFolderCommand { get { return _newFolderCommand; } }

        private readonly BlockableCommand<object> _newCommand;
        private readonly BlockableCommand<object> _openCommand;
        private readonly BlockableCommand<object> _addCommand;
        private readonly BlockableCommand<object> _newFolderCommand;

        private ILibraryView _library;
        private IErrorHandlerView _errorHandlerView;
        private IErrorHandler _errorHandler;
        private IFileValidator _fileValidator;
        private readonly ImbSettings _appSettings;
        private readonly IEventAggregator _eventAggregator;
        private DisplayVm _displayView;

        public event PropertyChangedEventHandler PropertyChanged;

        public IErrorHandlerView ErrorHandlerView { get { return _errorHandlerView; } }

        public DisplayVm DisplayView
        {
            get { return _displayView; }
            set
            {
                if (Equals(value, _displayView)) return;
                _displayView = value;
                OnPropertyChanged();
            }
        }

        public MainVm(ILibraryLocationDialog locationDialog, ILibrarySelector librarySelector, IErrorHandlerView errorHandler, IFileValidator validator, ImbSettings appSettings, IEventAggregator eventAggregator)
        {
            _locationDialog = locationDialog;
            _librarySelector = librarySelector;
            _errorHandlerView = errorHandler;
            _errorHandler = _errorHandlerView.ErrorHandler;
            _fileValidator = validator;
            _appSettings = appSettings;
            _eventAggregator = eventAggregator;

            _newCommand = new BlockableCommand<object>(false, NewLibrary);
            _openCommand = new BlockableCommand<object>(false, OpenLibrary);
            _addCommand = new BlockableCommand<object>(true, AddFile);
            _newFolderCommand = new BlockableCommand<object>(true, NewFolder);

            DropHandler.Current.SetErrorHandler(_errorHandler);
            DropHandler.Current.SetFileValidator(_fileValidator);
        }

        private void NewFolder(object obj)
        {
            _eventAggregator.SendMessage(new AddNewFolder(null));
        }

        private void AddFile(object obj)
        {
            string newFileLocation = null;
            try
            {
                newFileLocation = _locationDialog.GetFileLocation();
                if (newFileLocation != null)
                {
                    _library.Operations.AddFile(newFileLocation);
                }
            }
            catch (Exception e)
            {
                var message = newFileLocation == null 
                    ? "Unable to determine file path."
                    : string.Format("Unable to load file {0}", newFileLocation);
                _errorHandler.LogError("Add file failed", message);
            }
        }

        private void NewLibrary(object obj)
        {
            string libraryLocation = null;
            try
            {
                libraryLocation = _locationDialog.GetNewLibraryLocation(_appSettings.LibraryPath);
                if (libraryLocation != null)
                {
                    Library = _librarySelector.CreateLibrary(libraryLocation);
                    CreateNewDisplayView();
                    AllowLibraryCommands();
                    _appSettings.LibraryPath = libraryLocation;
                }
            }
            catch (Exception e)
            {
                var message = libraryLocation == null
                    ? "Unable to determine file path."
                    : string.Format("Unable to create library {0}", libraryLocation);
                _errorHandler.LogError("Add file failed", message);
            }
        }

        private void CreateNewDisplayView()
        {
            DisplayView = new DisplayVm(Library.LoadedBinariesCache);
            Library.AttachDisplay(DisplayView);
        }

        private void OpenLibrary(object obj)
        {
            string libraryLocation = null;
            try
            {
                libraryLocation = _locationDialog.GetExistingLibraryLocation(_appSettings.LibraryPath);
                if (libraryLocation != null)
                {
                    Library = _librarySelector.OpenLibrary(libraryLocation);
                    CreateNewDisplayView();
                    AllowLibraryCommands();
                    _appSettings.LibraryPath = libraryLocation;
                }
            }
            catch (Exception e)
            {
                var message = libraryLocation == null
                    ? "Unable to determine file path."
                    : string.Format("Unable to open library {0}", libraryLocation);
                _errorHandler.LogError("Add file failed", message);
            }
        }

        private void AllowLibraryCommands()
        {
            _addCommand.Blocked = false;
            _newFolderCommand.Blocked = false;
        }

        public ILibraryView Library
        {
            get { return _library; }
            set
            {
                if (Equals(value, _library)) return;
                if (_library != null)
                    _librarySelector.CloseLibrary(_library);

                _library = value;
                DropHandler.Current.SetLibrary(_library);
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AppClosing()
        {
        }
    }
}
