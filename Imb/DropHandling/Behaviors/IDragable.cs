using System;

namespace Imb.DropHandling.Behaviors
{
    interface IDragable
    {
        /// <summary>
        /// Type of the data item
        /// </summary>
        Type DataType { get; }

        /// <summary>
        /// Dropped an item from the collection
        /// </summary>
        void DroppedItem(object i);
    }
}
