using System;
using System.Collections.Generic;
using System.Text;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class FeedItemEqualityComparer : IEqualityComparer<FeedItem>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(FeedItem x, FeedItem y)
        {
            return x?.Link?.ToLowerInvariant().Trim() == y?.Link?.ToLowerInvariant().Trim();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(FeedItem obj)
        {
            return obj.Link.GetHashCode();
        }
    }
}
