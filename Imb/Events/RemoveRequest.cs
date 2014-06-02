using System;

namespace Imb.Events
{
    public sealed class RemoveRequest
    {
        public Guid Id { get; private set; }

        public RemoveRequest(Guid id)
        {
            Id = id;
        }
    }
}