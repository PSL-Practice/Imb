using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;
using Imb.Annotations;
using Imb.Caching;
using Imb.ErrorHandling;
using Imb.EventAggregation;
using Imb.Events;
using Imb.Utils;
using Imb.ViewModels;
using Utils.CollectionUtilities;
using Utils.ObservableCollectionViewers;

namespace Imb.Data.View
{
    public class LibraryView : INotifyPropertyChanged, IObservableCollectionAccessMediator, 
        IDisposable, ILibraryView, IListener<NodeSelfSelect>, IListener<RemoveRequest>
    {
        private readonly object _lock = new object();
        private readonly ITagCache _tags;
        private readonly IPathsCache _paths;
        private readonly IBinariesCache _binariesCache;
        private readonly Dispatcher _dispatcher;
        private readonly IEventAggregator _eventAggregator;
        private string _location;
        public event PropertyChangedEventHandler PropertyChanged;
        private PropertyOrderedObservableCollectionViewer<LibraryViewNode> _nodeViewer;
        private ObservableCollection<LibraryViewNode> _sourceNodes;
        private bool _stop;
        private Dictionary<Guid, LibraryViewNode> _nodeIndex = new Dictionary<Guid, LibraryViewNode>();
        private IDisplay _display;
        private LibraryViewNode _selectedItem;

        public bool Loaded { get; set; }

        public LibraryContentOperations Operations { get; private set; } 

        public string Location
        {
            get { return _location; }
            set
            {
                if (value == _location) return;
                _location = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Do not edit this collection, it is purely a thread safe view of the root nodes. All updates to the view are made automatically
        /// when items are edited. To edit the actual collection, see <see cref="Items"/>.
        /// </summary>
        public INotifyCollectionChanged View
        {
            get { return _nodeViewer.View; }
        }

        /// <summary>
        /// This is the source collection for the <see cref="View"/> property, which can be safely edited.
        /// </summary>
        public ObservableCollection<LibraryViewNode> Items
        {
            get { return _sourceNodes; }
        }

        public LibraryViewNode SelectedItem
        {
            get { return _selectedItem; }
            private set
            {
                if (Equals(value, _selectedItem)) return;

                if (_selectedItem != null)
                    _selectedItem.IsSelected = false;

                _selectedItem = value;
                
                if (_selectedItem != null) 
                    _selectedItem.IsSelected = true;

                _display.Show(_selectedItem);
                OnPropertyChanged();
            }
        }

        public LoadedBinaryCache LoadedBinariesCache { get; set; }

        public LibraryView(ITagCache tags, IPathsCache paths, IBinariesCache binaries, Dispatcher dispatcher, ILibraryOperations libraryOps, IErrorHandler errorHandler, IFileValidator fileValidator, IEventAggregator eventAggregator)
        {
            _tags = tags;
            _paths = paths;
            _binariesCache = binaries;
            _dispatcher = dispatcher;
            _eventAggregator = eventAggregator;
            _sourceNodes = new ObservableCollection<LibraryViewNode>();
            _nodeViewer = new PropertyOrderedObservableCollectionViewer<LibraryViewNode>(_dispatcher);
            _nodeViewer.SetMediator(this);
            _nodeViewer.Attach(_sourceNodes);
            _nodeViewer.OrderBy(f => f.Name, new [] {"Name"}, false);
            
            _eventAggregator.AddListener(this, _dispatcher);
            LoadedBinariesCache = new LoadedBinaryCache(100, binaries);

            Operations = new LibraryContentOperations(libraryOps, this, errorHandler, fileValidator, _dispatcher, _eventAggregator);

            _dispatcher.BeginInvoke(new Action(PerformLoad));
        }

        private void PerformLoad()
        {
            Task.Factory.StartNew(PerformInitialLoad);
        }

        public void AttachDisplay(IDisplay display)
        {
            _display = display;
        }

        private void PerformInitialLoad()
        {
            foreach (var binary in _binariesCache.Binaries)
            {
                if (_stop) break;
                AddNode(binary);
            }

            Loaded = true;
        }

        private void AddNode(BinaryStorageClass binary)
        {
            var destination = FindParentNode(binary.PathId);
            var collection = destination == null ? _sourceNodes : destination.Children;

            var node = MakeFileNode(binary);
            node.Parent = destination;
            collection.Add(node);
        }

        private LibraryFileNode MakeFileNode(BinaryStorageClass binary)
        {
            var parent = FindParentNode(binary.PathId);
            var pathStrings = GetPathStrings(_paths.Get(binary.PathId));
            var fileNode = new LibraryFileNode(_dispatcher, _eventAggregator)
            {
                Name = _tags.Get(binary.NameTag),
                Id = binary.Id,
                Path = pathStrings,
                Parent = parent, 
            };

            _nodeIndex[binary.Id] = fileNode;
            return fileNode;
        }

        private string[] GetPathStrings(int[] pathTags)
        {
            return pathTags == null ? new string[]{} : pathTags.Select(t => _tags.Get(t)).ToArray();
        }

        private LibraryFolderNode FindParentNode(int pathId)
        {
            if (pathId == 0) return null;

            var parent = _sourceNodes as IList<LibraryViewNode>;
            LibraryFolderNode folderNode = null;
            LibraryFolderNode lastParent = null;
            var path = _paths.Get(pathId);
            if (path != null)
            {
                foreach (var folder in path)
                {
                    var name = _tags.Get(folder);
                    folderNode = parent.FirstOrDefault(p => p is LibraryFolderNode && p.Name == name)  as LibraryFolderNode;
                    if (folderNode == null)
                    {
                        folderNode = MakeFolderNode(name);
                        folderNode.Parent = lastParent;
                        parent.Add(folderNode);
                        _nodeIndex[folderNode.Id] = folderNode;
                    }
                    parent = folderNode.Children;
                    lastParent = folderNode;
                }
                
            }
            return folderNode;
        }

        private LibraryFolderNode MakeFolderNode(string name)
        {
            return new LibraryFolderNode(_dispatcher, _eventAggregator) { Id = Guid.NewGuid(), Name = name };
        }

        private IList<LibraryViewNode> FindParentNodeCollection(int pathId)
        {
            var node = FindParentNode(pathId);
            return node == null ? _sourceNodes : node.Children;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SafeAccessCollection(INotifyCollectionChanged collection, Action actionToRunSafely)
        {
            lock (_lock)
                actionToRunSafely();
        }

        public void Dispose()
        {
            _stop = true;
        }

        public LibraryViewNode MakeNode(BinaryStorageClass binary)
        {
            var libraryFileNode = MakeFileNode(binary);
            return libraryFileNode;
        }

        public LibraryViewNode MakeNode(string[] path, string name)
        {
            var libraryFolderNode = MakeFolderNode(name);
            libraryFolderNode.Path = path;
            return libraryFolderNode;
        }

        public void DeleteNode(Guid id)
        {
            LibraryViewNode node;
            if (_nodeIndex.TryGetValue(id, out node))
            {
                var container = node.Parent != null ? node.Parent.Children : _sourceNodes;
                container.Remove(node);
                _nodeIndex.Remove(id);
            }
        }

        public void AddNode(LibraryViewNode node)
        {
            if (node.Path != null && node.Path.Any())
            {
                var pathId = _paths.AddOrGet(node.Path);
                node.Parent = FindParentNode(pathId);
            }

            var container = node.Parent == null ? _sourceNodes : node.Parent.Children;
            container.Add(node);
            _nodeIndex[node.Id] = node;
            SelectedItem = node;
        }

        public IEnumerable<LibraryViewNode> GetAllChildren(Guid id)
        {
            LibraryViewNode startNode;
            if (!_nodeIndex.TryGetValue(id, out startNode))
                yield break;

            foreach (var child in GetAllChildren(startNode))
            {
                yield return child;
            }
        }

        private IEnumerable<LibraryViewNode> GetAllChildren(LibraryViewNode startNode)
        {
            foreach (var node in startNode.Children)
            {
                if (node is LibraryFileNode)
                    yield return node;
                else
                {
                    foreach (var child in GetAllChildren(node))
                    {
                        yield return child;
                    }
                }
            }
        }

        public LibraryViewNode Get(Guid id)
        {
            LibraryViewNode item;
            return _nodeIndex.TryGetValue(id, out item) ? item : null;
        }

        public void Handle(NodeSelfSelect message)
        {
            SelectedItem = message.Node;
        }

        public void Handle(RemoveRequest message)
        {
            if (SelectedItem != null)
                _eventAggregator.SendMessage(new RemoveDocumentById(SelectedItem.Id));
        }
    }
}
