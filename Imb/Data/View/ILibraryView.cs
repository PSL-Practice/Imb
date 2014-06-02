using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Imb.Caching;
using Imb.ViewModels;

namespace Imb.Data.View
{
    public interface ILibraryView
    {
        LibraryViewNode MakeNode(BinaryStorageClass binary);
        LibraryViewNode MakeNode(string[] path, string name);
        LibraryViewNode Get(Guid id);
        void DeleteNode(Guid id);
        void AddNode(LibraryViewNode node);
        IEnumerable<LibraryViewNode> GetAllChildren(Guid id);
        LibraryContentOperations Operations { get; }
        INotifyCollectionChanged View { get; }
        ObservableCollection<LibraryViewNode> Items { get; }
        LibraryViewNode SelectedItem { get; }
        LoadedBinaryCache LoadedBinariesCache { get; set; }
        void AttachDisplay(IDisplay display);
    }
}