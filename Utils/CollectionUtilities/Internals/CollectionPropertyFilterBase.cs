using System;
using System.ComponentModel;

namespace Utils.CollectionUtilities.Internals
{
    public abstract class CollectionPropertyFilterBase<TSource, TDest>: IDisposable
        where TSource : class, INotifyPropertyChanged
        where TDest : class, INotifyPropertyChanged
    {
        protected Action<TSource, TDest> RestoreItemHandler { get; private set; }
        protected Action<TSource, TDest> HideItemHandler { get; private set; }
        public Func<TSource, TDest, bool> Filter { get; private set; }

        public CollectionPropertyFilterBase(Action<TSource, TDest> restoreItemHandler, Action<TSource, TDest> hideItemHandler, Func<TSource, TDest, bool> filter)
        {
            RestoreItemHandler = restoreItemHandler;
            HideItemHandler = hideItemHandler;
            Filter = filter;
        }

        public virtual void NewItem(TSource sourceItem, TDest destItem, out bool display)
        {
            display = Filter(sourceItem, destItem);
        }

        public abstract void RemoveItem(TSource item);
        public abstract void Dispose();
    }
}