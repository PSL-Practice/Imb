using Imb.Data.View;

namespace Imb.Events
{
    public sealed class MoveRequest
    {
        public LibraryViewNode Node { get; private set; }
        public LibraryViewNode NewParent { get; set; }

        public MoveRequest(LibraryViewNode node, LibraryViewNode newParent)
        {
            Node = node;
            NewParent = newParent;
        }
    }
}