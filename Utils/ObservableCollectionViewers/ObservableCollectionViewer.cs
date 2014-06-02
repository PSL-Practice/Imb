using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Threading;

namespace Utils.ObservableCollectionViewers
{
    /// <summary>
    /// This class allows an observable collection that is being populated on background threads to be "viewed" from the UI thread.
    /// 
    /// The class exposes a copy of the ObservableCollection that is updated only on the UI thread (via a dispatcher) in response to
    /// INotifyCollectionChanged events that could come from any thread.
    /// 
    /// The order of items in the view will correspond with the order in the original.
    /// </summary>
    public sealed class ObservableCollectionViewer<T> : FilteringOrderUndefinedObservableCollectionViewer<T, T> where T : class
    {
        public ObservableCollectionViewer(Dispatcher dispatcher) : base(dispatcher)
        {
        }

        public ObservableCollectionViewer(Dispatcher dispatcher, IList<T> viewedCollection) : base(dispatcher, viewedCollection)
        {
            Attach(viewedCollection);
        }

        public ObservableCollectionViewer(Dispatcher dispatcher, Func<T, T, bool> filter) : base(dispatcher, filter)
        {
        }

        public ObservableCollectionViewer(Dispatcher dispatcher, IList<T> viewedCollection, Func<T, T, bool> filter) : base(dispatcher, viewedCollection, filter)
        {
            Attach(viewedCollection);
        }

        protected override T MakeItem(T sourceItem)
        {
            return sourceItem;
        }

        protected override void RespondToCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Move)
                MoveItems(args);
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

    }
}