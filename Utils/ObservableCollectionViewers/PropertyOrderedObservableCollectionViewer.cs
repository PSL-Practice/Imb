using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;
using Utils.CollectionUtilities;
using Utils.CollectionUtilities.Internals;

namespace Utils.ObservableCollectionViewers
{
    /// <summary>
    /// This class allows an observable collection that is being populated on background threads to be "viewed" from the UI thread.
    /// 
    /// The class exposes a copy of the ObservableCollection that is updated only on the UI thread (via a dispatcher) in response to
    /// INotifyCollectionChanged events that could come from any thread.
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
    /// </summary>
    public sealed class PropertyOrderedObservableCollectionViewer<T> : FilteringOrderUndefinedObservableCollectionViewer<T, T> where T : class, INotifyPropertyChanged
    {
        private IObservableCollectionAccessMediator _mediator;
        private CollectionOrderBase<T> _collectionOrder;
        private CollectionPropertyFilter<T, T> _filter;

        public PropertyOrderedObservableCollectionViewer(Dispatcher dispatcher)
            : base(dispatcher)
        {
        }

        public PropertyOrderedObservableCollectionViewer(Dispatcher dispatcher, IList<T> viewedCollection,
                                                         IObservableCollectionAccessMediator mediator)
            : base(dispatcher, viewedCollection)
        {
            _mediator = mediator;
            Attach(viewedCollection);
        }

        public void SetMediator(IObservableCollectionAccessMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Attach(IList<T> viewedCollection)
        {
            if (_mediator == null)
                throw new MediatorNotSetException();

            base.Attach(viewedCollection);
        }

        protected override T MakeItem(T sourceItem)
        {
            return sourceItem;
        }

        protected override void RespondToCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Move)
            {
                if (_collectionOrder == null) MoveItems(args);
            }
            else
                base.RespondToCollectionChanged(args);
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

        protected override T AddItem(T newItem, int index)
        {
            if (_collectionOrder != null)
                index = _collectionOrder.AttachToItemAndDetermineCorrectIndex(newItem);
            return base.AddItem(newItem, index);
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
        public void OrderBy<TKey>(Func<T, TKey> function, IEnumerable<string> orderCheckPropertyNames, bool orderDescending) where TKey : IComparable
        {
            if (_mediator == null) throw new MediatorNotSetException();

            if (_collectionOrder != null) _collectionOrder.Detach();

            var order = new CollectionOrder<T, TKey>(function, View, _dispatcher, orderCheckPropertyNames, orderDescending);
            order.LoadKeysAndTriggerArrange(_target, _mediator);
            _collectionOrder = order;
        }

        private T UnfilteredAddItem(T item, int originalIndex)
        {
            int index = -1;
            try
            {
                index = _collectionOrder == null ? originalIndex : _collectionOrder.AttachToItemAndDetermineCorrectIndex(item);
                base.AddItem(item, item, index);
                return item;
            }
            catch (Exception e)
            {
                Debug.Print("Exception while adding item {0} @ {1}: {2}", item == null ? "NULL" : item.ToString(), index, e);

                throw;
            }
        }

        protected override T RemoveItem(T item)
        {
            if (_filter != null)
                _filter.RemoveItem(item);
            return UnfilteredRemoveItem(item);
        }

        protected override void HideItem(T source)
        {
            UnfilteredRemoveItem(source);
        }

        private T UnfilteredRemoveItem(T item)
        {
            var actualItem = base.RemoveItem(item);
            _collectionOrder.RemoveItem(actualItem);
            return actualItem;
        }

        public override void AttachFilter(Func<T,T,bool> filter)
        {
            AttachFilter(filter, null);
        }

        public void AttachFilter(Func<T, T, bool> filter, IEnumerable<string> properties)
        {
            _filter = new CollectionPropertyFilter<T, T>(PropertyFilterRestoreItem, PropertyFilterHideItem, filter, properties);
            bool unused;
            SafeForEach(i => _filter.NewItem(i, GetDestFromSource(i), out unused));
            base.AttachFilter(filter);
        }

        private void PropertyFilterHideItem(T item, T dest)
        {
            UnfilteredRemoveItem(item);
        }

        private void PropertyFilterRestoreItem(T item, T dest)
        {
            UnfilteredAddItem(item, 0);
        }
    }
}
