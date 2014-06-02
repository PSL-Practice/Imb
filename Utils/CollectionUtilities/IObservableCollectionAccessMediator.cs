using System;
using System.Collections.Specialized;

namespace Utils.CollectionUtilities
{
    /// <summary>
    /// This interface must be implemented to provide <see cref="CollectionOrder{T, TKey}"/> with a mechanism to 
    /// access a collection it is viewing while the collection is in a static state i.e. the implementation must be able to execute an
    /// action that uses the collection, and ensure that the collection does not change in any way while the action is in progress.
    /// 
    /// <see cref="CollectionOrder{T, TKey}"/> is unable to carry out functions against the whole collection (such as
    /// iterating over every item) if the collection could change on another thread. For example, iterators will throw exceptions if 
    /// the collection changes while iteration occurs, or the viewer may need to extract a consistent set of keys from the collection in
    /// order to sort the collection.
    /// </summary>
    public interface IObservableCollectionAccessMediator
    {
        /// <summary>
        /// Ensure that the collection specified does not change while executing the specified action. This could be implented using a lock.
        /// </summary>
        /// <param name="collection">The target collection. This allows a single implementation of the interface to protect more than one
        /// collection.</param>
        /// <param name="actionToRunSafely">The action that acts on the collection specified.</param>
        void SafeAccessCollection(INotifyCollectionChanged collection, Action actionToRunSafely);
    }
}