using System.Windows.Threading;
using Imb.EventAggregation;
using Imb.Events;

namespace Imb.Data.View
{
    public class LibraryFolderNode : LibraryViewNode
    {
        private bool _initialEditMode;

        public LibraryFolderNode(Dispatcher dispatcher, IEventAggregator eventAggregator)
            : base(dispatcher, eventAggregator)
        {
            _newFolderCommand.Blocked = false;
        }

        public override string ToString()
        {
            return string.Format("LibraryFolderNode {{{0}}}", Name);
        }

        public void EnterInitialNameMode()
        {
            _initialEditMode = true;
            BeginEditLabel.Execute(null);
        }

        protected override void EndEditLabel(bool saveEdit)
        {
            base.EndEditLabel(saveEdit);
            if (_initialEditMode)
            {
                if (!saveEdit)
                    EventAggregator.SendMessage(new RemoveDocumentById(Id));
                _initialEditMode = false;
            }
        }
    }
}