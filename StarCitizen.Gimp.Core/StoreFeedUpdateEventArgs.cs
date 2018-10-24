using System;
using System.Collections.Generic;
using System.Text;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class StoreFeedUpdateEventArgs : BandwidthEventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<StoreItem> Items { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public StoreFeedUpdateEventArgs() :
            base()
        {
            Items = new StoreItem[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="totalContentLength"></param>
        /// <param name="contentLength"></param>
        public StoreFeedUpdateEventArgs(IEnumerable<StoreItem> items, long totalContentLength, long contentLength) :
            base(totalContentLength, contentLength)
        {
            Items = items ?? new StoreItem[0];
        }
    }
}
