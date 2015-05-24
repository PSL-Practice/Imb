using System;
using System.Linq;
using System.Text;
using Imb.EventAggregation;
using Imb.Events;

namespace TestImb.Mocks
{
    public class EventListener : IListener<RenameRequest>, IListener<RemoveRequest>, IListener<AddNewFolder>, IListener<NodeSelfSelect>, IListener<MoveRequest>, IListener<RemoveDocumentById>
    {
        private StringBuilder sb = new StringBuilder();

        public string Requests
        {
            get { return sb.ToString(); }
        }

        public void Handle(RenameRequest message)
        {
            sb.AppendLine(String.Format("Rename {0} {1}", TestIds.Id(message.Id), message.NewName));
        }

        public void Handle(RemoveRequest message)
        {
            sb.AppendLine("Remove selected");
        }

        public void Handle(AddNewFolder message)
        {
            sb.AppendLine(String.Format("AddFolder {0}", (message.Path == null || !message.Path.Any())? string.Empty : message.Path.Aggregate((t, i) => t + @"\" + i)));
        }

        public void Handle(NodeSelfSelect message)
        {
            sb.AppendLine(String.Format("Select {0}", TestIds.Id(message.Node.Id)));
        }

        public void Handle(MoveRequest message)
        {
            sb.AppendLine(String.Format("Move {0} to {1}", message.Node, message.NewParent));
        }

        public void Handle(RemoveDocumentById message)
        {
            sb.AppendLine(String.Format("Remove {0}", TestIds.Id(message.Id)));
        }
    }
}