using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Utils.CollectionUtilities
{
    /// <summary>
    /// This class monitors an <see cref="ObservableCollection"/> for order changes, and listens to the property changing event on the current top item,
    /// and when a field changes, notifies all listening items, via an event, that the best value in the collection has changed.
    /// 
    /// This class is not thread aware and should only be used from the user interface thread. Its intended use is to allow elements of an ordered collection
    /// to react to changes to the value of the first item. For example, if all items are required to expose a percentage value that represents
    /// their value in relation to the value of the top item, this class would enable them to react to changes to the top item's value by recalculating 
    /// their percentage values.
    /// 
    /// To use the class, create an instance for the type held in the <see cref="ObservableCollection"/>, and call <see cref="AttachCollection"/> passing
    /// the target collection. You must also supply two actions which respectively attach and detach the tracker from members of the
    /// target collection.
    /// 
    /// Members of the target collection must add handlers for the tracker's <see cref="Recalculate"/> event. This handler will be called back
    /// whenever the item at the top of the list changes value, or a different instance becomes the top of the list.
    /// </summary>
    public class HighValueTracker<T> where T : class, INotifyPropertyChanged
    {
        private Action<T> _attachAction;
        private Action<T> _detachAction;
        private ObservableCollection<T> _collection;
        private T _topItem;
        private List<T> _attachedItems = new List<T>();
        private List<string> _propertyNames;

        public event EventHandler<RecalculateArgs<T>> Recalculate;

        public void AttachCollection(ObservableCollection<T> collection, Action<T> attachAction, Action<T> detachAction, IEnumerable<string> significantPropertyNames = null)
        {
            if (ReferenceEquals(_collection, collection) 
                && ReferenceEquals(detachAction, _detachAction)
                && ReferenceEquals(attachAction, _attachAction)
                && _propertyNames == null 
                && significantPropertyNames == null) return;

            _propertyNames = significantPropertyNames != null ? significantPropertyNames.ToList() : null;

            SwitchCollection(collection);
            AttachAllItems(collection, attachAction, detachAction);
            AttachToTopItem(collection.Any() ? collection[0] : null);
        }

        private void AttachToTopItem(T topItem)
        {
            if (_topItem != null)
            {
                _topItem.PropertyChanged -= TopItemChanged;
                _topItem = null;
            }

            _topItem = topItem;
            if (_topItem != null)
            {
                _topItem.PropertyChanged += TopItemChanged;

                RaiseTopItemChanged();
            }
        }

        private void TopItemChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_propertyNames == null || _propertyNames.Contains(e.PropertyName))
                RaiseTopItemChanged();
        }

        private void RaiseTopItemChanged()
        {
            var handler = Recalculate;
            if (handler != null) handler(this, new RecalculateArgs<T>(_topItem));
        }

        private void AttachAllItems(ObservableCollection<T> collection, Action<T> attachAction, Action<T> detachAction)
        {
            _attachAction = attachAction;
            _detachAction = detachAction;
            AttachToAddedItems(collection);
        }

        private void AttachToAddedItems(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                _attachAction(item);
                _attachedItems.Add(item);
            }
        }

        private void SwitchCollection(ObservableCollection<T> collection)
        {
            if (_collection != null)
            {
                _collection.CollectionChanged -= CollectionChangeHandler;
                DetachFromRemovedItems(_collection);
            }

            _collection = collection;
            _collection.CollectionChanged += CollectionChangeHandler;
        }

        private void DetachFromRemovedItems(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                _detachAction(item);
                _attachedItems.Remove(item);
            }
        }

        private void CollectionChangeHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool forceRecalc = false;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AttachToAddedItems(e.NewItems.Cast<T>());
                    forceRecalc = true;
                    break;

                case NotifyCollectionChangedAction.Remove:
                    DetachFromRemovedItems(e.OldItems.Cast<T>());
                    break;

                case NotifyCollectionChangedAction.Replace:
                    DetachFromRemovedItems(e.OldItems.Cast<T>());
                    AttachToAddedItems(e.NewItems.Cast<T>());
                    forceRecalc = true;
                    break;

                case NotifyCollectionChangedAction.Reset:
                    DetachFromRemovedItems(_attachedItems.ToList());
                    break;
                
            }

            var collectionTop = _collection.FirstOrDefault();
            if (!ReferenceEquals(collectionTop, _topItem))
                AttachToTopItem(collectionTop);
            else
            {
                if (forceRecalc)
                    RaiseTopItemChanged();
            }
        }
    }

    public class RecalculateArgs<T> : EventArgs
    {
        public T Item { get; private set; }

        public RecalculateArgs(T item)
        {
            Item = item;
        }
    }
}
