using System.Collections.Generic;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class RssFeedUpdateEventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<FeedItem> Items { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public RssFeedUpdateEventArgs()
        {
            Items = new FeedItem[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        public RssFeedUpdateEventArgs(IEnumerable<FeedItem> items)
        {
            Items = items ?? new FeedItem[0];
        }
    }
}
