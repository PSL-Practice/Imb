using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;

namespace Utils.CollectionUtilities.Internals
{
    /// <summary>
    /// This class maintains items in a sorted order in an ObservableCollection.
    /// 
    /// The ordering of the collection will be checked whenever one of the items raises a property changed event
    /// (see <see cref="INotifyPropertyChanged"/>) for one of a set of specified properties. Only the position of the element
    /// raising the event will be considered.
    ///
    /// The class does not up to collection changed events and must be informed when a new item is added to the collection, 
    /// <see cref="AttachToItemAndDetermineCorrectIndex"/>, and when items are removed from the collection, <see cref="RemoveItem"/>.
    /// 
    /// The additiona and removal behaviour could be automated in this class, but it is a component within controller instances that manage this 
    /// process. It was not originally a stand-alone class, but the behaviour is useful and became required in more than one type of controller,
    /// hence it had to be made more generally available.
    /// </summary>
    internal class CollectionOrder<T, TKey> : CollectionOrderBase<T>
        where T : class, INotifyPropertyChanged
        where TKey : IComparable
    {
        private class ItemKey
        {
            public T Item;
            public TKey Key;

            public ItemKey(T item, TKey key)
            {
                Item = item;
                Key = key;
            }
        }

        private object _lock = new object();
        private readonly Func<T, TKey> _keyGetter;
        private IList<T> _list;
        private List<ItemKey> _keys;
        private ObservableCollection<T> _view;
        private Dispatcher _orderDispatcher;
        private readonly bool _orderDescending;
        private List<string> _propertyNames;
        private bool _attached;

        /// <summary>
        /// Construct an <see cref="CollectionOrder{T,TKey}"/>.
        /// </summary>
        /// <param name="keyGetter">The function extracts the sort key from an instance of <see cref="T"/>.</param>
        /// <param name="view">The collection.</param>
        /// <param name="dispatcher">The UI dispatcher through which collection changes can be performed.</param>
        /// <param name="orderCheckPropertyNames">The names of the properties that should trigger a check.</param>
        /// <param name="orderDescending">True if the collection should be in descending order.</param>
        public CollectionOrder(Func<T, TKey> keyGetter, ObservableCollection<T> view, Dispatcher dispatcher,
                               IEnumerable<string> orderCheckPropertyNames, bool orderDescending)
        {
            if (view == null)
                throw new ArgumentException("A view must be provided.", "view");

            _keyGetter = keyGetter;
            _view = view;
            _orderDispatcher = dispatcher;
            _orderDescending = orderDescending;
            _propertyNames = orderCheckPropertyNames.ToList();
            _attached = true;
        }

        public override void LoadKeysAndTriggerArrange(INotifyCollectionChanged target, IObservableCollectionAccessMediator mediator)
        {
            lock (_lock)
            {
                if (target != null)
                {
                    _list = target as IList<T>;
                    if (_list == null) throw new NonListMonitoringIsNotSupportedException();
                    mediator.SafeAccessCollection(target, GetKeys);

                    var sorter = new Func<Func<ItemKey, TKey>, List<ItemKey>>(
                        s => _orderDescending
                            ? _keys.OrderByDescending(s).ToList()
                            : _keys.OrderBy(s).ToList());
                    _keys = sorter(v => v.Key);
                    _orderDispatcher.BeginInvoke(new Action(ArrangeItems));

                }
                else
                {
                    _keys = new List<ItemKey>();
                    _orderDispatcher.BeginInvoke(new Action(ArrangeItems));
                }
            }
        }

        private void ArrangeItems()
        {
            lock (_lock)
            {
                var index = 0;
                foreach (var pair in _keys)
                {
                    if (!_view.Contains(pair.Item))
                    {
                        _view.Insert(index, pair.Item);
                    }
                    else
                    {
                        var oldIndex = _view.IndexOf(pair.Item);
                        if (oldIndex != index)
                            _view.Move(oldIndex, index);
                    }
                    ++index;
                }
            }
        }

        private void GetKeys()
        {
            var keys = new List<ItemKey>();
            foreach (var item in _list)
            {
                item.PropertyChanged += OnItemChanged;
                var key = _keyGetter(item);
                keys.Add(new ItemKey(item, key));
            }
            _keys = keys;
        }

        private void OnItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_attached)
            {
                var performChange = new Action(() => PerformItemChanged(sender as T, e));
                if (_orderDispatcher.CheckAccess())
                {
                    performChange();
                }
                else
                {
                    _orderDispatcher.BeginInvoke(performChange);
                }
            }
        }

        private void PerformItemChanged(T item, PropertyChangedEventArgs e)
        {
            lock (_lock)
            {
                if (_attached)
                {
                    if (_propertyNames.Contains(e.PropertyName))
                    {
                        var keyPair = FindExisting(item);
                        if (keyPair != null)
                        {
                            var key = _keyGetter(item);
                            var oldIndex = _keys.IndexOf(keyPair);
                            var correctIndex = DetermineIndex(key);

                            keyPair.Key = key;

                            if (correctIndex != oldIndex)
                            {
                                if (oldIndex < correctIndex) --correctIndex;
                                _view.Move(oldIndex, correctIndex);
                                _keys.RemoveAt(oldIndex);

                                _keys.Insert(correctIndex, keyPair);
                            }
                        }
                    }
                }
            }
        }

        private ItemKey FindExisting(T item)
        {
            return _keys.FirstOrDefault(p => ReferenceEquals(p.Item, item));
        }

        private int DetermineIndex(TKey key)
        {
            var correctIndex = _orderDescending
                                   ? _keys.FindIndex(p => p.Key.CompareTo(key) <= 0)
                                   : _keys.FindIndex(p => p.Key.CompareTo(key) >= 0);
            if (correctIndex < 0)
                correctIndex = _keys.Count;
            return correctIndex;
        }

        public override void Detach()
        {
            lock (_lock)
            {
                _attached = false;
                _view = null;
                foreach (var itemKey in _keys)
                {
                    itemKey.Item.PropertyChanged -= OnItemChanged;
                }
            }
        }

        public override int AttachToItemAndDetermineCorrectIndex(T newItem)
        {
            newItem.PropertyChanged += OnItemChanged;
            try
            {
                var key = _keyGetter(newItem);
                var pair = new ItemKey(newItem, key);
                var index = DetermineIndex(key);
                _keys.Insert(index, pair);
                return index;
            }
            catch (Exception)
            {
                newItem.PropertyChanged -= OnItemChanged;
                throw;
            }
        }

        public override void RemoveItem(T item)
        {
            var pair = FindExisting(item);
            _keys.Remove(pair);
        }
    }
}