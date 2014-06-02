using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Utils.CollectionUtilities.Internals
{
    public sealed class CollectionPropertyFilter<TSource, TDest> : CollectionPropertyFilterBase<TSource, TDest> 
        where TSource : class, INotifyPropertyChanged
        where TDest : class, INotifyPropertyChanged
    {
        private object _lock = new object();
        private Dictionary<TSource, bool> _knownItems = new Dictionary<TSource, bool>();
        private Dictionary<TSource, TDest> _itemLookup = new Dictionary<TSource, TDest>();
        private Dictionary<TDest, TSource> _destLookup = new Dictionary<TDest, TSource>();
        private readonly List<string> _properties;

        public CollectionPropertyFilter(Action<TSource, TDest> restoreItemHandler, Action<TSource, TDest> hideItemHandler, Func<TSource, TDest, bool> filter, IEnumerable<string> properties) : base(restoreItemHandler, hideItemHandler, filter)
        {
            _properties = (properties ?? new string[] {}).ToList();
        }

        public override void RemoveItem(TSource item)
        {
            lock (_lock)
            {
                _knownItems.Remove(item);
            }
            StopMonitoringItem(item);

        }

        public override void Dispose()
        {
            Dictionary<TSource, bool> knownItems;
            lock (_lock)
            {
                knownItems = _knownItems;
                _knownItems = null;
            }

            if (knownItems != null)
            {
                foreach (var knownItem in knownItems.Keys)
                {
                    StopMonitoringItem(knownItem);
                }
            }
        }

        public override void NewItem(TSource item, TDest dest, out bool display)
        {
            base.NewItem(item, dest, out display);
            MonitorItem(item, dest, display);
        }

        private void MonitorItem(TSource item, TDest dest, bool display)
        {
            lock (_lock)
            {
                _knownItems[item] = display;
                _itemLookup[item] = dest;
                _destLookup[dest] = item;
            }
            item.PropertyChanged += OnItemChangeNotification;
            dest.PropertyChanged += OnItemChangeNotification;
        }

        private void StopMonitoringItem(TSource item)
        {
            TDest dest;
            if (_itemLookup.TryGetValue(item, out dest))
            {
                dest.PropertyChanged -= OnItemChangeNotification;
            }
            item.PropertyChanged -= OnItemChangeNotification;
        }

        private void OnItemChangeNotification(object sender, PropertyChangedEventArgs e)
        {
            var source = sender as TSource;
            var dest = sender as TDest;
            if (source == null && dest == null) return;

            if (source != null)
            {
                if (!_itemLookup.TryGetValue(source, out dest))
                    return;
            }
            else
            {
                if (!_destLookup.TryGetValue(dest, out source))
                    return;
            }

            if (_properties.Contains(e.PropertyName) || !_properties.Any())
            {
                var display = Filter(source, dest);
                bool previousDisplay;
                if (_knownItems.TryGetValue(source, out previousDisplay) && previousDisplay == display) return;
 
                _knownItems[source] = display;
                if (display)
                {
                    RestoreItemHandler(source, dest);
                }
                else
                {
                    HideItemHandler(source, dest);
                }
            }
        }
    }
}