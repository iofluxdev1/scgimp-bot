using System.Collections.Generic;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class StoreFeedUpdateEventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<StoreItem> Items { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public StoreFeedUpdateEventArgs()
        {
            Items = new StoreItem[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        public StoreFeedUpdateEventArgs(IEnumerable<StoreItem> items)
        {
            Items = items ?? new StoreItem[0];
        }
    }
}
