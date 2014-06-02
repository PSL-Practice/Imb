using System;

namespace Imb.Events
{
    public sealed class RenameRequest
    {
        public Guid Id { get; private set; }
        public string NewName { get; private set; }

        public RenameRequest(Guid id, string newName)
        {
            Id = id;
            NewName = newName;
        }
    }
}