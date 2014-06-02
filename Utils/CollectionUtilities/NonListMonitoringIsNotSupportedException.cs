using System;
using System.Runtime.Serialization;

namespace Utils.CollectionUtilities
{
    /// <summary>
    /// This exception is thrown when a collection requested to be monitored by <see cref="PropertyOrderedObservableCollectionViewer{T}"/>
    /// does not implement IList.
    /// </summary>
    [Serializable]
    public class NonListMonitoringIsNotSupportedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public NonListMonitoringIsNotSupportedException() : base("Non-IList type INotifyCollectionChanged implementations cannot be monitored.")
        {
        }

        protected NonListMonitoringIsNotSupportedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}