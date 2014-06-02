using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Utils.CollectionUtilities
{
    public class ObservableUnion<T> : ObservableCollection<T>
    {
        private List<ObservableCollection<T>> _collections = new List<ObservableCollection<T>>();

        public ObservableUnion(ObservableCollection<T> first, ObservableCollection<T> second,
                               ObservableCollection<T> third = null, ObservableCollection<T> fourth = null)
        {
            if (first != null)
                _collections.Add(first);
            if (second != null)
                _collections.Add(second);
            if (third != null)
                _collections.Add(third);
            if (fourth != null)
                _collections.Add(fourth);

            foreach (var collection in _collections)
            {
                lock (collection)
                {
                    Include(collection);
                }
            }
        }

        private void Include(ObservableCollection<T> collection)
        {
            foreach (var item in collection)
                Add(item);
            collection.CollectionChanged += CollectionChangeHandler;
        }

        private void CollectionChangeHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_collections.Any(c => ReferenceEquals(sender, c)))
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var newItem in e.NewItems.Cast<T>())
                        Add(newItem);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var removedItem in e.OldItems.Cast<T>())
                        Remove(removedItem);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (var removedItem in e.OldItems.Cast<T>())
                        Remove(removedItem);
                    foreach (var newItem in e.NewItems.Cast<T>())
                        Add(newItem);
                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in this.Distinct().ToArray())
                    {
                        var localItem = item;
                        var requiredCount = _collections.Where(c => !ReferenceEquals(c, sender)).SelectMany(c => c).Count(i => localItem.Equals(i));
                        var numberToDelete = this.Count(i => localItem.Equals(i)) - requiredCount;
                        for (var deleteCount = 0; deleteCount < numberToDelete; ++deleteCount)
                            Remove(item);
                    }
                    
                    break;

                case NotifyCollectionChangedAction.Move:
                    //the union does not guarantee to preserve the order of the unioned collections.
                    break;
            }
        }

        public void AddCollection(ObservableCollection<T> newCollection)
        {
            _collections.Add(newCollection);
            Include(newCollection);
        }
    }
}
