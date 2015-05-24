using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Imb.Annotations;
using Imb.DropHandling.Behaviors;
using Imb.EventAggregation;
using Imb.Events;
using Utils.CollectionUtilities;
using Utils.ObservableCollectionViewers;
using Utils.UISupport;

namespace Imb.Data.View
{
    public abstract class LibraryViewNode : INotifyPropertyChanged, IObservableCollectionAccessMediator, IDragable, IDropable
    {
        protected IEventAggregator EventAggregator { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        
        private object _lock = new object();
        private string _name;
        private ObservableCollection<LibraryViewNode> _children;
        private LibraryViewNode _parent;
        private Dispatcher _dispatcher;
        private PropertyOrderedObservableCollectionViewer<LibraryViewNode> _childViewer;
        private bool _editLabel;
        private readonly SimpleCommand<object> _editLabelCommand;
        private readonly SimpleCommand<object> _finishEditLabelCommand;
        private readonly SimpleCommand<object> _cancelEditLabelCommand;
        private readonly SimpleCommand<object> _removeNodeCommand;
        protected readonly BlockableCommand<object> _newFolderCommand;
        private bool _isSelected;
        private string _editName;

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public string EditName
        {
            get { return _editName; }
            set
            {
                if (value == _editName) return;
                _editName = value;
                OnPropertyChanged();
            }
        }

        public LibraryViewNode Parent
        {
            get { return _parent; }
            set
            {
                if (Equals(value, _parent)) return;
                _parent = value;
                OnPropertyChanged();
            }
        }

        public bool EditLabel
        {
            get { return _editLabel; }
            set
            {
                if (value.Equals(_editLabel)) return;
                _editLabel = value;

                OnPropertyChanged();
            }
        }

        public IList<LibraryViewNode> Children {get { return _children; }}

        public ObservableCollection<LibraryViewNode> ChildrenView
        {
            get { return _childViewer.View; }
        }

        public Guid Id { get; set; }
        public string[] Path { get; set; }

        public ICommand BeginEditLabel { get { return _editLabelCommand; } }
        public ICommand FinishEditLabel { get { return _finishEditLabelCommand; } }
        public ICommand CancelEditLabel { get { return _cancelEditLabelCommand; } }
        public ICommand RemoveNode { get { return _removeNodeCommand; } }
        public ICommand NewFolder { get { return _newFolderCommand; } }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value.Equals(_isSelected)) return;
                _isSelected = value;

                if (!_isSelected)
                    CancelSelection();
                OnPropertyChanged();
            }
        }

        private void CancelSelection()
        {
            if (_editLabel)
                EndEditLabel(false);
        }

        protected LibraryViewNode(Dispatcher dispatcher, IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            _children = new ObservableCollection<LibraryViewNode>();
            _childViewer = new PropertyOrderedObservableCollectionViewer<LibraryViewNode>(dispatcher);
            _childViewer.SetMediator(this);
            _childViewer.Attach(_children);
            _childViewer.OrderBy(c => c.Name, new []{"Name"}, false);
            _editLabelCommand = new SimpleCommand<object> {ExecuteDelegate = PerformEditLabel};
            _finishEditLabelCommand = new SimpleCommand<object> { ExecuteDelegate = o => EndEditLabel(true) };
            _cancelEditLabelCommand = new SimpleCommand<object> {ExecuteDelegate = o => EndEditLabel(false)};
            _removeNodeCommand = new SimpleCommand<object> {ExecuteDelegate = PerformRemoveNode};
            _newFolderCommand = new BlockableCommand<object>(true, PerformNewFolder);
        }

        private void PerformRemoveNode(object obj)
        {
            EndEditLabel(false);
            EventAggregator.SendMessage(new RemoveDocumentById(Id));
        }

        private void PerformNewFolder(object obj)
        {
            EndEditLabel(false);
            EventAggregator.SendMessage(new AddNewFolder(GetPathForChild()));
        }

        protected virtual void EndEditLabel(bool saveEdit)
        {
            if (!EditLabel) return;

            if (saveEdit)
            {
                Name = _editName;
                EventAggregator.SendMessage(new RenameRequest(Id, Name));
            }
            EditLabel = false;
        }

        private void PerformEditLabel(object o)
        {
            EditName = Name;
            EditLabel = true;
            EventAggregator.SendMessage(new NodeSelfSelect(this));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public string[] GetPathForChild()
        {
            var namePath = new[] {Name};
            return Path == null ? namePath : Path.Concat(namePath).ToArray();
        }

        public void SafeAccessCollection(INotifyCollectionChanged collection, Action actionToRunSafely)
        {
            lock (_lock)
                actionToRunSafely();
        }

        public Type DataType { get { return typeof (LibraryViewNode); } }

        public void Drop(object data, int index = -1)
        {
            var item = data as LibraryViewNode;
            if (item != null)
                EventAggregator.SendMessage(new MoveRequest(item, this));
            Debug.Print("Dropzors");
        }

        public void DroppedItem(object i)
        {
        }
    }
}