using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Threading;
using Utils.Properties;

namespace Utils.ObservableCollectionViewers
{
    /// <summary>
    /// This class allows an observable collection that is being populated on background threads to be "viewed" from the UI thread.
    /// 
    /// The class exposes a copy of the ObservableCollection that is updated only on the UI thread (via a dispatcher) in response to
    /// INotifyCollectionChanged events that could come from any thread.
    /// 
    /// The "view" of the collection will be maintained such that it contains the same items as the original. However the order of the items
    /// in the view is not defined and will not correspond to the order of objects in the original collection. Users should have no expectation
    /// that the order of the objects will follow any particular pattern or remain static, as this is an aspect of the class that can be customised
    /// and subclasses of this base class will likely dictate how the view's items are ordered.
    /// 
    /// This base class is abstract and cannot be directly instatiated.
    /// </summary>
    public abstract class OrderUndefinedObservableCollectionViewer<TSource, TItem> : INotifyPropertyChanged where TItem : class
    {
        private Dictionary<TSource, TItem> _itemLookup = new Dictionary<TSource, TItem>();
        protected readonly Dispatcher _dispatcher;
        public ObservableCollection<TItem> View { get; set; }

        protected INotifyCollectionChanged _target;

        /// <summary>
        /// Construct a collection viewer to which a collection can be attached.
        /// </summary>
        public OrderUndefinedObservableCollectionViewer(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            View = new ObservableCollection<TItem>();
        }

        /// <summary>
        /// Construct a collection viewer and attach a collection.
        /// 
        /// See <see cref="Attach"/>.
        /// </summary>
        /// <param name="viewedCollection">The collection to be attached.</param>
        protected OrderUndefinedObservableCollectionViewer(Dispatcher dispatcher, IList<TSource> viewedCollection) : this(dispatcher)
        {
        }

        /// <summary>
        /// Ensure that the target collection cannot change while Attach runs, as it is used to construct the view collection and the construction is not thread safe.
        /// </summary>
        /// <param name="viewedCollection">The collection to view. This must be a collection, and if updates are to be reflected in the view, it must also implement 
        /// <see cref="INotifyCollectionChanged"/>.</param>
        public virtual void Attach(IList<TSource> viewedCollection)
        {
            if (_target != null)
            {
                _target.CollectionChanged -= OnCollectionChanged;
            }

            if (viewedCollection != null)
            {
                Dispatch(() =>
                             {
                                 ClearView();
                                 foreach (var sourceItem in viewedCollection)
                                 {
                                     AddItem(sourceItem, View.Count);
                                 }

                                 _target = viewedCollection as INotifyCollectionChanged;
                                 if (_target != null)
                                     _target.CollectionChanged += OnCollectionChanged;
                                 OnPropertyChanged("View");
                             });
            }

        }
        
        /// <summary>
        /// Run the supplied action on the dispacther's thread.
        /// </summary>
        /// <param name="action">The processing to perform on the UI thread.</param>
        protected void Dispatch(Action action)
        {
            if (!_dispatcher.CheckAccess())
                _dispatcher.BeginInvoke(action);
            else
                action();
        }

        protected void SafeForEach(Action<TSource> action)
        {
            var list = _target as IList<TSource>;
            if (list != null)
            {
                foreach (var item in list)
                {
                    action(item);
                }
            }
        }


        protected abstract TItem MakeItem(TSource sourceItem);

        /// <summary>
        /// Ensure that an existing item in the source collection is visible. It will have previously been hidden by a superclass.
        /// </summary>
        /// <param name="source">The item to be displayed.</param>
        protected virtual void RefreshItem(TSource source)
        {
            if (!_itemLookup.ContainsKey(source))
            {
                AddItem(source, View.Count);
            }
        }

        /// <summary>
        /// Remove an item that is present in the source collection from the view.  
        /// </summary>
        /// <param name="source"></param>
        protected virtual void HideItem(TSource source)
        {
            if (_itemLookup.ContainsKey(source))
                RemoveItem(source);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _dispatcher.BeginInvoke(new DispatcherOperationCallback(HandleCollectionChanged), e);
        }

        private object HandleCollectionChanged(object o)
        {
            var e = o as NotifyCollectionChangedEventArgs;
            if (e != null)
            {
                RespondToCollectionChanged(e);
            }

            return null;
        }

        /// <summary>
        /// Override this method to change behaviour associated with handling collection change notifications.
        /// 
        /// Please note that the Unordered base class should not respond to <see cref="NotifyCollectionChangedAction.Move"/>
        /// events.
        /// </summary>
        /// <param name="args">The original notifcation event arguments.</param>
        protected virtual void RespondToCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
                AddItems(args);
            else if (args.Action == NotifyCollectionChangedAction.Remove)
                RemoveItems(args);
            else if (args.Action == NotifyCollectionChangedAction.Reset)
                Reset(args);
            else if (args.Action == NotifyCollectionChangedAction.Replace)
                ReplaceItems(args);
        }

        private void Reset(NotifyCollectionChangedEventArgs args)
        {
            ClearView();
        }

        protected virtual void ClearView()
        {
            View.Clear();
            _itemLookup.Clear();
        }

        private void AddItems(NotifyCollectionChangedEventArgs args)
        {
            var index = args.NewStartingIndex;
            foreach (var newItem in args.NewItems)
            {
                AddItem((TSource)newItem, index++);
            }
        }

        /// <summary>
        /// Add an item to the view. If this is overridden, the responsibility for adding the item to the
        /// view is accepted by the caller, however, this responsibilty can be returned to the base class
        /// by calling the base version. This may be the best course if the objective is to change the position
        /// in the list in which the item is added.
        /// 
        /// If the subclass requires the item to be omitted from the collection, this will have no adverse effect. In that case,
        /// the method should return null to indicate that no new item has been added to the collection.
        /// </summary>
        /// <param name="newItem">The item to be added.</param>
        /// <param name="index">The index at which the item can be added.</param>
        /// <returns>The new item in the collection, or null if the item was not added.</returns>
        protected virtual TItem AddItem(TSource newItem, int index)
        {
            var mirrorItem = MakeItem(newItem);
            _itemLookup[newItem] = mirrorItem;
            View.Insert(index, mirrorItem);
            return mirrorItem;
        }
        
        /// <summary>
        /// Return the corresponding destination instance for a given source instance, if it is a known value.
        /// </summary>
        /// <param name="source">The source instance.</param>
        /// <returns>The destination instance, or null if the source item is not known.</returns>
        protected virtual TItem GetDestFromSource(TSource source)
        {
            TItem dest;
            _itemLookup.TryGetValue(source, out dest);
            return dest;
        }

        /// <summary>
        /// Add an item to the view. If this is overridden, the responsibility for adding the item to the
        /// view is accepted by the caller, however, this responsibilty can be returned to the base class
        /// by calling the base version. This may be the best course if the objective is to change the position
        /// in the list in which the item is added.
        /// 
        /// If the subclass requires the item to be omitted from the collection, this will have no adverse effect.
        /// </summary>
        /// <param name="newItem">The item to be added.</param>
        /// <param name="index">The index at which the item can be added.</param>
        protected virtual void AddItem(TSource sourceItem, TItem newItem, int index)
        {
            _itemLookup[sourceItem] = newItem;
            View.Insert(index, newItem);
        }

        private void RemoveItems(NotifyCollectionChangedEventArgs args)
        {
            foreach (var newItem in args.OldItems)
            {
                RemoveItem((TSource)newItem);
            }
        }

        /// <summary>
        /// Remove an item from the view. If this is overridden, the responsibility for removing the item from thre
        /// view is accepted by the caller. However, this responsibility can be returned by calling the base implementation.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        protected virtual TItem RemoveItem(TSource item)
        {
            TItem mirrorItem = FindMatchingItem(item);
            if (mirrorItem != null)
            {
                _itemLookup.Remove(item);
                View.Remove(mirrorItem);
            }
            return mirrorItem;
        }

        private TItem FindMatchingItem(TSource item)
        {
            TItem mirror;
            if (_itemLookup.TryGetValue(item, out mirror))
                return mirror;
            return null;
        }

        private void ReplaceItems(NotifyCollectionChangedEventArgs args)
        {
            RemoveItems(args);
            AddItems(args);
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}