using System;
using System.Collections.Specialized;
using Utils.CollectionUtilities;

namespace Utils.ObservableCollectionViewers
{
    /// <summary>
    /// This is a minimal mediator that may be used when the initial load of an observable collection viewer
    /// is guaranteed not to be concurrent with other activities on the collection.
    /// </summary>
    public class NullMediator : IObservableCollectionAccessMediator
    {
        public void SafeAccessCollection(INotifyCollectionChanged collection, Action actionToRunSafely)
        {
            actionToRunSafely();
        }
    }
}
