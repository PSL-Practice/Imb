using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Imb.Caching;
using Imb.Data;
using Imb.Data.View;
using Imb.EventAggregation;
using Imb.ViewModels;

namespace TestImb.Mocks
{
    public class MockLibraryView : ILibraryView
    {
        private readonly Dispatcher _dispatcher;
        private readonly MockLibraryOperations _mockLibraryOps;
        private readonly LibraryContentOperations _libraryOps;

        public MockLibraryView(Dispatcher dispatcher, MockLibraryOperations mockLibraryOps, MockErrorHandler errorHandler, 
            MockFileValidator fileValidator, string path, UnitTestEventAggregator eventAggregator)
        {
            Items = new ObservableCollection<LibraryViewNode>();
            _eventAggregator = eventAggregator;
            _dispatcher = dispatcher;
            _mockLibraryOps = mockLibraryOps;
            _libraryOps = new LibraryContentOperations(_mockLibraryOps, this, errorHandler, fileValidator, _dispatcher, _eventAggregator);
            Path = path;
        }

        public string Path { get; set; }

        public string Print()
        {
            var sb = new StringBuilder();

            foreach (var item in Items)
            {
                Print(sb, item, 0);
            }

            return sb.ToString();
        }

        public ObservableCollection<LibraryViewNode> Items { get; set; }

        private UnitTestEventAggregator _eventAggregator;

        public LibraryViewNode FindFirstNode(string name)
        {
            return FindFirstNode(name, Items);
        }

        private LibraryViewNode FindFirstNode(string name, IList<LibraryViewNode> items)
        {
            foreach (var node in items)
            {
                if (node.Name == name) return node;
                var child = FindFirstNode(name, node.Children);
                if (child != null) return child;
            }

            return null;
        }

        private void Print(StringBuilder sb, LibraryViewNode item, int indent)
        {
            var indentString = new string(' ', indent*4);
            sb.AppendLine(string.Format("{0}{1}", indentString, item));
            foreach (var child in item.Children)
            {
                Print(sb, child, indent + 1);
            }
        }

        private LibraryViewNode FindParentNode(string[] path)
        {
            return FindParentNode(path, Items, null);
        }

        private LibraryViewNode FindParentNode(string[] path, IList<LibraryViewNode> items, LibraryViewNode parentNode)
        {
            if (path.Length == 0) return parentNode;

            var parent = items.FirstOrDefault(i => i.Name == path[0] && i is LibraryFolderNode);
            if (parent == null)
            {
                parent = new LibraryFolderNode(_dispatcher, _eventAggregator) { Id = Guid.NewGuid(), Name = path[0], Parent = parentNode, Path = parentNode == null ? null : parentNode.GetPathForChild()};
                items.Add(parent);
            }

            return FindParentNode(path.Skip(1).ToArray(), parent.Children, parent);
        }

        public LibraryViewNode MakeNode(BinaryStorageClass binary)
        {
            return _mockLibraryOps.MakeFileNode(binary, _dispatcher);
        }

        public LibraryViewNode MakeNode(string[] path, string name)
        {
            return new LibraryFolderNode(_dispatcher, _eventAggregator) {Id = Guid.NewGuid(), Name = name, Path = path};
        }

        public LibraryViewNode Get(Guid id)
        {
            return FindNode(id, Items);
        }

        public void DeleteNode(Guid id)
        {
            var node = Get(id);
            if (node.Parent != null)
            {
                node.Parent.Children.Remove(node);
            }
            else
            {
                Items.Remove(node);
            }
        }

        public void AddNode(LibraryViewNode node)
        {
            node.Parent = FindParentNode(node.Path);
            if (node.Parent == null)
            {
                Items.Add(node);
            }
            else
            {
                node.Parent.Children.Add(node);
            }
        }

        public IEnumerable<LibraryViewNode> GetAllChildren(Guid id)
        {
            var startNode = FindNode(id, Items);
            return GetAllChildren(startNode);
        }

        public LibraryContentOperations Operations { get { return _libraryOps; } }
        
        INotifyCollectionChanged ILibraryView.View { get { return null; } }

        public LibraryViewNode SelectedItem { get; private set; }

        public LoadedBinaryCache LoadedBinariesCache { get; set; }
        public void AttachDisplay(IDisplay display)
        {
            
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

        private LibraryViewNode FindNode(Guid id, IList<LibraryViewNode> items)
        {
            var item = items.FirstOrDefault(i => i.Id == id);
            if (item != null)
                return item;

            foreach (var child in items)
            {
                item = FindNode(id, child.Children);
                if (item != null) return item;
            }

            return null;
        }
    }
}