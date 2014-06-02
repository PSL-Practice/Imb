using System;
using System.Runtime.Serialization;
using Utils.CollectionUtilities;

namespace Utils.ObservableCollectionViewers
{
    /// <summary>
    /// This exception is thrown when a collection is attached to a <see cref="PropertyOrderedObservableCollectionViewer{T}"/> without 
    /// a mediator set (see <see cref="IObservableCollectionAccessMediator"/>). 
    /// </summary>
    [Serializable]
    public class MediatorNotSetException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public MediatorNotSetException() : base("Mediator not set.")
        {
        }

        public MediatorNotSetException(string message) : base(message)
        {
        }

        protected MediatorNotSetException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}