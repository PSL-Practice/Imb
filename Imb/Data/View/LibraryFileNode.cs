using System;
using System.Windows.Threading;
using Imb.EventAggregation;

namespace Imb.Data.View
{
    public class LibraryFileNode : LibraryViewNode
    {
        private byte[] _bits;

        public byte[] Bits
        {
            get { return _bits; }
            set
            {
                if (Equals(value, _bits)) return;
                _bits = value;
                OnPropertyChanged();
            }
        }

        public LibraryFileNode(Dispatcher dispatcher, IEventAggregator eventAggregator) : base(dispatcher, eventAggregator)
        {
            _newFolderCommand.Blocked = true;
        }

        public override string ToString()
        {
            return string.Format("LibraryFileNode {{{0}}}", Name);
        }
    }
}