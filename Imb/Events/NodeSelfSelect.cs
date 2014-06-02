using Imb.Data.View;

namespace Imb.Events
{
    public sealed class NodeSelfSelect
    {
        public LibraryViewNode Node { get; private set; }

        public NodeSelfSelect(LibraryViewNode node)
        {
            Node = node;
        }
    }
}