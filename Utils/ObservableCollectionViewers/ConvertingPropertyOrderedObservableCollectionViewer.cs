using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;
using Utils.CollectionUtilities;
using Utils.CollectionUtilities.Internals;
using Utils.DebugUtils;

namespace Utils.ObservableCollectionViewers
{
    /// <summary>
    /// This class allows an observable collection that is being populated on background threads to be "viewed" from the UI thread, and the items
    /// are transformed into a new type when they are displayed in the view.
    /// 
    /// The class exposes a copy of the ObservableCollection that is updated only on the UI thread (via a dispatcher) in response to
    /// INotifyCollectionChanged events that could come from any thread. Items in the view are of a different type to the source collection <see cref="TItem"/>.
    /// 
    /// Items in the view are converted from items in the source collection by a function supplied on the constructor.
    /// 
    /// Items in the view will be ordered according to a function. When the collection is built, the function will be used to sort the
    /// view. Users will be able to supply the name of the properties that can trigger a check of an items position in the collection. When
    /// an item raises a property change notification (see <see cref="INotifyPropertyChanged"/>) for the nominated property, the function
    /// will be used to locate the item's correct position in the collection and the item will be moved <b>in the view only</b>. Please note
    /// that the original collection will not be changed.  
    /// 
    /// When the ordering function is changed or initially set, the view must be re-ordered to match it. This process must be able
    /// to handle multi-threaded access. To assist with obtaining the initial set of keys, the target collection must be accompanied by an
    /// implementation of <see cref="IObservableCollectionAccessMediator"/>, an interface that will allow the viewer to access the 
    /// target collection while it is in a static state. 
    /// 
    /// Any change to the key returned by an instance must be accompanied by a property changed event. Therefore, if the initial sort is 
    /// performed on stale data, the order will be corrected when the property changed event is processed on the UI thread.
    /// 
    /// It should be noted that, by definition, all view ordering must take place on the UI thread, or else exceptions will be
    /// thrown in client code. When a key changes, the new key will be determined on the thread sending the INotifyPropertyChanged,
    /// and the value will be handed over to the UI thread before it is applied. All ordering of the data is carried out using
    /// copies of the keys, so that the UI thread does not have to access the item properties directly. This prevents the UI thread receiving
    /// keys that may change during the sort process. 
    /// 
    /// This also implies that if the items in the collection have compound keys, the key deriving function will always return a consistent key,
    /// i.e. that the deriving function is thread safe with respect to the item. The easiest way to achieve this is if the actual construction
    /// of the key is performed by the item so that it can use a lock to guarantee consistency.
    /// 
    /// The viewer does not demand that keys be unique. The order of items with identical keys is not defined and should not be relied upon.
    public sealed class ConvertingPropertyOrderedObservableCollectionViewer<TSource, TItem> : OrderUndefinedObservableCollectionViewer<TSource, TItem>
        where TSource : class, INotifyPropertyChanged
        where TItem : class, INotifyPropertyChanged
    {
        private readonly Func<TSource, TItem> _converter;
        private IObservableCollectionAccessMediator _mediator;
        private CollectionOrderBase<TItem> _collectionOrder;
        private IQuickLog _log = new QuickLog();
        private CollectionPropertyFilterBase<TSource, TItem> _filter; 

        public ConvertingPropertyOrderedObservableCollectionViewer(Func<TSource, TItem> converter, Dispatcher dispatcher) : base(dispatcher)
        {
            _log.Add("Converter constructor called");
            _converter = converter;
        }

        public ConvertingPropertyOrderedObservableCollectionViewer(Dispatcher dispatcher, IList<TSource> viewedCollection, 
                                                                   Func<TSource, TItem> converter, IObservableCollectionAccessMediator mediator) : base(dispatcher, viewedCollection)
        {
            _log.Add("Collection constructor called");
            _converter = converter;
            _mediator = mediator;
            Attach(viewedCollection);
        }


        public void SetMediator(IObservableCollectionAccessMediator mediator)
        {
            _log.Add("Mediator set to type {0}", mediator.GetType());
            _mediator = mediator;
        }

        public override void Attach(IList<TSource> viewedCollection)
        {
            _log.Add("Attach called with type {0}", viewedCollection.GetType());
            if (_mediator == null)
                throw new MediatorNotSetException();

            base.Attach(viewedCollection);
        }

        protected override TItem MakeItem(TSource sourceItem)
        {
            _log.LazyAdd("Making item from {0}", new Lazy<string>(sourceItem.ToString));
            return _converter(sourceItem);
        }

        protected override void RespondToCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            _log.LazyAdd("Responding to collection changed {0}", new Lazy<string>(args.ToString));
            if (args.Action == NotifyCollectionChangedAction.Move)
            {
                if (_collectionOrder == null) MoveItems(args);
            }
            else
                base.RespondToCollectionChanged(args);
        }

        protected override void ClearView()
        {
            base.ClearView();

            if (_collectionOrder != null)
                _collectionOrder.LoadKeysAndTriggerArrange(View, _mediator);
        }

        private void MoveItems(NotifyCollectionChangedEventArgs args)
        {
            var from = args.OldStartingIndex;
            var to = args.NewStartingIndex;
            for (int x = 0; x < args.OldItems.Count; ++x)
            {
                View.Move(from++, to++);
            }
        }

        protected override TItem AddItem(TSource sourceItem, int originalIndex)
        {
            _log.LazyAdd("AddItem {0} @ {1}", new Lazy<string>(sourceItem.ToString), originalIndex);
            
            TItem newItem = null;
            bool display = true;

            try
            {
                newItem = _converter(sourceItem);
                if (_filter != null)
                    _filter.NewItem(sourceItem, newItem, out display);
            }
            catch (Exception e)
            {
                Debug.Print("Exception while filtering items ({0} and {1}) : {2}", sourceItem == null ? "NULL" : sourceItem.ToString(), newItem == null ? "NULL" : newItem.ToString(), e);
                Debug.Print(_log.ToString());

                throw;
            }

            if (display)
                return UnfilteredAddItem(sourceItem, newItem, originalIndex);

            return null;
        }

        private TItem UnfilteredAddItem(TSource sourceItem, TItem newItem, int originalIndex)
        {
            int index = -1;
            try
            {
                _log.LazyAdd("Add Converted {0} @ {1}", new Lazy<string>(newItem.ToString), originalIndex);
                index = _collectionOrder == null ? originalIndex : _collectionOrder.AttachToItemAndDetermineCorrectIndex(newItem);
                base.AddItem(sourceItem, newItem, index);
                _log.LazyAdd("Post add collection : {0}", View.Select(v => v.ToString()).Aggregate((t, i) => t + "@@@" + i));
                return newItem;
            }
            catch (Exception e)
            {
                Debug.Print("Exception while adding item {0} @ {1}: {2}", newItem == null ? "NULL" : newItem.ToString(), index, e);
                Debug.Print(_log.ToString());

                throw;
            }
        }

        protected override TItem RemoveItem(TSource item)
        {
            if (_filter != null)
                _filter.RemoveItem(item);
            return UnfilteredRemoveItem(item);
        }

        protected override void HideItem(TSource source)
        {
            UnfilteredRemoveItem(source);
        }

        private TItem UnfilteredRemoveItem(TSource item)
        {
            _log.LazyAdd("Remove {0}", new Lazy<string>(item.ToString));
            var actualItem = base.RemoveItem(item);
            _collectionOrder.RemoveItem(actualItem);
            return actualItem;
        }

        public void AttachFilter(Func<TSource, TItem, bool> filter)
        {
            AttachFilter(filter, null);
        }

        public void AttachFilter(Func<TSource, TItem, bool> filter, IEnumerable<string> properties)
        {
            _filter = new CollectionPropertyFilter<TSource, TItem>(PropertyFilterRestoreItem, PropertyFilterHideItem, filter, properties);
            bool unused;
            SafeForEach(i => _filter.NewItem(i, GetDestFromSource(i), out unused));
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_target is IList<TSource>)
            {
                var filterer = new Action(() =>
                {
                    foreach (var sourceItem in _target as IList<TSource>)
                    {
                        if (_filter.Filter(sourceItem, GetDestFromSource(sourceItem)))
                            RefreshItem(sourceItem);
                        else
                            HideItem(sourceItem);
                    }
                });

                if (!_dispatcher.CheckAccess())
                {
                    _dispatcher.BeginInvoke(filterer);
                }
                else
                {
                    filterer();
                }
            }
        }

        private void PropertyFilterHideItem(TSource source, TItem destItem)
        {
            Dispatch(() => UnfilteredRemoveItem(source));
        }

        private void PropertyFilterRestoreItem(TSource source, TItem destItem)
        {
            Dispatch(() => UnfilteredAddItem(source, destItem, 0));
        }

        /// <summary>
        /// Specify a function that extracts a sortable key from object instances, that can be used to order items in the view.
        /// 
        /// The ordering of the view will be checked whenever one of the items raises a property changed event
        /// (see <see cref="INotifyPropertyChanged"/>) for one of a set of specified properties. Only the position of the element
        /// raising the event will be considered.
        /// 
        /// When new items are added to the collection, the function will be used to select an appropriate position
        /// in the view.
        /// </summary>
        /// <param name="function">The function that gets the sort key from an instance of <see cref="T"/>.</param>
        /// <param name="orderCheckPropertyNames">The names of the properties that should trigger a check.</param>
        /// <param name="orderDescending">True if the sort order is to be descending instead of ascending</param>
        public void OrderBy<TKey>(Func<TItem, TKey> function, IEnumerable<string> orderCheckPropertyNames, bool orderDescending) where TKey : IComparable
        {
            _log.LazyAdd("Set Order {0}", new Lazy<string>(function.ToString), new Lazy<string>(() => orderCheckPropertyNames.Aggregate((t, i) => t + "," + i)));
            if (_mediator == null) throw new MediatorNotSetException();

            if (_collectionOrder != null) _collectionOrder.Detach();

            var order = new CollectionOrder<TItem, TKey>(function, View, _dispatcher, orderCheckPropertyNames, orderDescending);
            order.LoadKeysAndTriggerArrange(View, _mediator);
            _collectionOrder = order;
        }

#if DEBUG
        public string GetLog()
        {
            return _log.ToString();
        }
#endif
    }
}