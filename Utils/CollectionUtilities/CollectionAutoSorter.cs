using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Threading;
using Utils.CollectionUtilities.Internals;

namespace Utils.CollectionUtilities
{
    /// <summary>
    /// This class monitors an observable collection and maintains its items in sort order defined by a function that returns a sort key.
    /// 
    /// Items added to the collection and items removed from the collection are monitored to ensure that the collection remains sorted.
    /// 
    /// ObservableCollections are typically part of the user interface and therefore elements may not be moved within the collection on
    /// any thread except the owning thread. The class therefore requires a Dispatcher through which it can invoke move operations to 
    /// obey this contstraint.
    /// 
    /// If items in the collection are moved "manually" (i.e. by some means other than by this class) then behaviour is undefined, and
    /// this should be avoided. 
    /// 
    /// If the items in the collection implement INotifyPropertyChanged (this is expected to be the most common usage), then they will be
    /// monitored for changes. The user must supply a list of the property names that effect ordering. Changes to properties not in the list
    /// will be ignored, but changes to properties that are in the list will cause the sorter to check the ordering of the element raising
    /// the change. 
    /// 
    /// On construction, a <see cref="IObservableCollectionAccessMediator"/> is used to ensure that the collection is in a stable state when
    /// the sorter initially sorts it. This is required because the collection may be accessed from multiple threads, and the initial sort cannot
    /// be made safely if concurrent access is allowed while it is carried out.  
    /// </summary>
    /// <typeparam name="TItem">The type of the item held in the collection.</typeparam>
    /// <typeparam name="TKey">The type of the sort key returned by the key extract function.</typeparam>
    public class CollectionAutoSorter<TItem, TKey>
        where TItem : class, INotifyPropertyChanged
        where TKey : IComparable
    {
        private readonly Func<TItem, TKey> _keyGetter;
        private readonly ObservableCollection<TItem> _collection;
        private readonly Dispatcher _dispatcher;
        private CollectionOrder<TItem, TKey> _order;

        /// <summary>
        /// Construct a sorter.
        /// </summary>
        /// <param name="keyGetter">The function that extracts a key from items in the list.</param>
        /// <param name="collection">The collection to be sorted.</param>
        /// <param name="dispatcher">The dispatcher on which move operations can be performed.</param>
        /// <param name="orderCheckTriggerProperties">The names of the properties of items that cause ordering to be checked when they change.</param>
        /// <param name="collectionAccessMediator">An object capable of preventing concurrent access to the target collection while the sorter performs the initial sort.</param>
        public CollectionAutoSorter(Func<TItem, TKey> keyGetter, ObservableCollection<TItem> collection, Dispatcher dispatcher, IEnumerable<string> orderCheckTriggerProperties, IObservableCollectionAccessMediator collectionAccessMediator, bool orderDescending)
        {
            _keyGetter = keyGetter;
            _collection = collection;
            _dispatcher = dispatcher;

            _collection.CollectionChanged += HandleCollectionChanged;
            _order = new CollectionOrder<TItem, TKey>(keyGetter, collection, dispatcher, orderCheckTriggerProperties, orderDescending);
            _order.LoadKeysAndTriggerArrange(collection, collectionAccessMediator);

        }

        private void HandleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    _dispatcher.BeginInvoke(new Action(() =>
                                                {
                                                    foreach (var newItem in e.NewItems)
                                                    {
                                                        var item = newItem as TItem;
                                                        var itemIx = _collection.IndexOf(item);
                                                        var correctIndex = _order.AttachToItemAndDetermineCorrectIndex(item);
                                                        if (itemIx != correctIndex)
                                                            _collection.Move(itemIx, correctIndex);
                                                    }
                                                }));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var oldItem in e.OldItems)
                    {
                        _order.RemoveItem(oldItem as TItem);
                    }
                    break;
            }
        }
    }
}
