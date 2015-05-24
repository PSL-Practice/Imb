using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Imb.ErrorHandling;
using Imb.EventAggregation;
using Imb.Events;
using Imb.Utils;

namespace Imb.Data.View
{
    public class LibraryContentOperations : IListener<RenameRequest>, IListener<RemoveDocumentById>, IListener<AddNewFolder>, IListener<MoveRequest>
    {
        private static readonly string[] NewFilePath = { "*New" };

        private readonly ILibraryOperations _library;
        private readonly ILibraryView _libraryView;
        private readonly IErrorHandler _errorHandler;
        private readonly IFileValidator _fileValidator;
        private readonly IEventAggregator _eventAggregator;

        public LibraryContentOperations(ILibraryOperations library, ILibraryView libraryView, IErrorHandler errorHandler, IFileValidator fileValidator, Dispatcher dispatcher, IEventAggregator eventAggregator)
        {
            _library = library;
            _libraryView = libraryView;
            _errorHandler = errorHandler;
            _fileValidator = fileValidator;
            _eventAggregator = eventAggregator;

            _eventAggregator.AddListener(this);

            Debug.Assert(_fileValidator != null);
            Debug.Assert(_errorHandler != null);
        }

        public void AddFile(byte[] data, string name, string[] path, DateTime fileDateUtc, string fileContainerPath)
        {
            var binary = _library.AddFile(data, name, path ?? NewFilePath, fileDateUtc, fileContainerPath);
            _libraryView.AddNode(_libraryView.MakeNode(binary));
        }

        public void AddFile(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);
            if (!_fileValidator.Validate(bytes))
            {
                _errorHandler.LogError("File is not valid", string.Format("Unable to load from {0}", filePath));
                return;
            }

            AddFile(bytes, Path.GetFileNameWithoutExtension(filePath), NewFilePath,
                File.GetCreationTime(filePath), Path.GetDirectoryName(filePath));
        }

        public void AddFolder(string[] path, string name)
        {
            var node = _libraryView.MakeNode(path, name);
            _libraryView.AddNode(node);
            _eventAggregator.SendMessage(new NodeSelfSelect(node));
        }

        public Task Delete(Guid id)
        {
            var node = _libraryView.Get(id);
            if (node is LibraryFileNode)
            {
                return Task.Factory.StartNew(() =>
                {
                    _library.DeleteFile(id);
                    _libraryView.DeleteNode(id);
                });
            }

            var allChildren = _libraryView.GetAllChildren(id).ToList();
            return Task.Factory.StartNew(() =>
            {
                foreach (var childNode in allChildren)
                {
                    _library.DeleteFile(childNode.Id);
                    _libraryView.DeleteNode(childNode.Id);
                }
                _libraryView.DeleteNode(id);
            });
        }

        public Task Rename(Guid id, string newName)
        {
            var node = _libraryView.Get(id);
            if (node is LibraryFileNode)
            {
                return Task.Factory.StartNew(() =>
                {
                    _library.Rename(id, newName);
                    node.Name = newName;
                });
            }

            var allChildren = _libraryView.GetAllChildren(id).ToList();
            return Task.Factory.StartNew(() =>
            {
                node.Name = newName;
                foreach (var libraryViewNode in allChildren)
                {
                    var path = GetPath(libraryViewNode.Parent);
                    _library.MoveFile(libraryViewNode.Id, path, libraryViewNode.Name);
                }
            });
        }

        public Task Move(LibraryViewNode node, LibraryViewNode newParent)
        {
            if (node is LibraryFileNode)
            {
                return Task.Factory.StartNew(() =>
                {
                    _library.MoveFile(node.Id, newParent.GetPathForChild(), node.Name);

                    if (node.Parent != null)
                        node.Parent.Children.Remove(node);
                    node.Parent = newParent;
                    newParent.Children.Add(node);
                });
            }

            if (node is LibraryFolderNode)
            {
                return Task.Factory.StartNew(() =>
                {
                    var adjuster = new PathAdjuster(node.Path, newParent.GetPathForChild());
                    foreach (var child in GetAllChildren(node))
                    {
                        var newPath = adjuster.AdjustPath(child.Path);
                        _library.MoveFile(child.Id, newPath, child.Name);
                    }

                    if (node.Parent != null)
                        node.Parent.Children.Remove(node);
                    else
                        _libraryView.Items.Remove(node);

                    node.Parent = newParent;
                    newParent.Children.Add(node);
                });
            }
            return null;
        }

        private IEnumerable<LibraryFileNode> GetAllChildren(LibraryViewNode node)
        {
            foreach (var child in node.Children)
            {
                if (child is LibraryFileNode)
                    yield return child as LibraryFileNode;
                else
                {
                    foreach (var childFile in GetAllChildren(child))
                    {
                        yield return childFile;
                    }
                }
            }
        }

        private string[] GetPath(LibraryViewNode libraryViewNode)
        {
            var currentPath = new[] {libraryViewNode.Name};
            return libraryViewNode.Parent != null
                ? GetPath(libraryViewNode.Parent).Concat(currentPath).ToArray()
                : currentPath;
        }

        public void Handle(RenameRequest message)
        {
            Rename(message.Id, message.NewName);
        }

        public void Handle(RemoveDocumentById message)
        {
            Delete(message.Id);
        }

        public void Handle(AddNewFolder message)
        {
            AddFolder(message.Path, "New Folder");
        }

        public void Handle(MoveRequest message)
        {
            Move(message.Node, message.NewParent);
        }
    }
}