using System;

namespace Imb.Events
{
    /// <summary>
    /// A request to remove an item identified by its Id
    /// </summary>
    public sealed class RemoveDocumentById
    {
        public Guid Id { get; set; }

        public RemoveDocumentById(Guid id)
        {
            Id = id;
        }
    }
}