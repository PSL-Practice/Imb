using System.Collections.Specialized;
using System.ComponentModel;

namespace Utils.CollectionUtilities.Internals
{
    internal abstract class CollectionOrderBase<T> where T : class, INotifyPropertyChanged
    {
        public abstract void LoadKeysAndTriggerArrange(INotifyCollectionChanged target, IObservableCollectionAccessMediator mediator);

        public abstract void Detach();

        public abstract int AttachToItemAndDetermineCorrectIndex(T newItem);

        public abstract void RemoveItem(T item);
    }
}