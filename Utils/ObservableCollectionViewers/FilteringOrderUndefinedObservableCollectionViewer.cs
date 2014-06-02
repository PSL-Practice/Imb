using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Utils.ObservableCollectionViewers
{
    public abstract class FilteringOrderUndefinedObservableCollectionViewer<TSource, TDest> : OrderUndefinedObservableCollectionViewer<TSource, TDest> where TDest : class
    {
        private Func<TSource, TDest, bool> _filterFn;

        protected FilteringOrderUndefinedObservableCollectionViewer(Dispatcher dispatcher) : base(dispatcher)
        {
        }

        protected FilteringOrderUndefinedObservableCollectionViewer(Dispatcher dispatcher, Func<TSource, TDest, bool> filter) : base(dispatcher)
        {
        }

        protected FilteringOrderUndefinedObservableCollectionViewer(Dispatcher dispatcher, IList<TSource> viewedCollection) : base(dispatcher, viewedCollection)
        {
        }

        protected FilteringOrderUndefinedObservableCollectionViewer(Dispatcher dispatcher, IList<TSource> viewedCollection, Func<TSource, TDest, bool> filter)
            : base(dispatcher, viewedCollection)
        {
        }

        public virtual void AttachFilter(Func<TSource, TDest, bool> filter)
        {
            _filterFn = filter;
            ApplyFilter();
        }

        protected override void AddItem(TSource sourceItem, TDest newItem, int index)
        {
            if (FilterOk(sourceItem))
                base.AddItem(sourceItem, newItem, index);
        }

        protected override TDest AddItem(TSource newItem, int index)
        {
            if (FilterOk(newItem))
                return base.AddItem(newItem, index);

            return null;
        }

        private void ApplyFilter()
        {
            if (_target is IList<TSource>)
            {
                var filterer = new Action(() =>
                {
                    foreach (var sourceItem in _target as IList<TSource>)
                    {
                        if (FilterOk(sourceItem))
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

        private bool FilterOk(TSource sourceItem)
        {
            var dest = GetDestFromSource(sourceItem);
            return _filterFn == null || _filterFn(sourceItem, dest);
        }

        private bool FilterOk(TSource sourceItem, TDest destItem)
        {
            return _filterFn == null || _filterFn(sourceItem, destItem);
        }
    }
}