using System;
using System.Collections.Generic;
using System.Text;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class RssFeedUpdateEventArgs : BandwidthEventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<FeedItem> Items { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public RssFeedUpdateEventArgs() :
            base()
        {
            Items = new FeedItem[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="totalContentLength"></param>
        /// <param name="contentLength"></param>
        public RssFeedUpdateEventArgs(IEnumerable<FeedItem> items, long totalContentLength, long contentLength) :
            base(totalContentLength, contentLength)
        {
            Items = items ?? new FeedItem[0];
        }
    }
}
