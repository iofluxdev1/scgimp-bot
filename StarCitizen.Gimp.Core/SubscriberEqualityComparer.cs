using System;
using System.Collections.Generic;
using System.Text;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class SubscriberEqualityComparer : IEqualityComparer<ScGimpSubscriber>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(ScGimpSubscriber x, ScGimpSubscriber y)
        {
            return x?.Email?.ToLowerInvariant().Trim() == y?.Email?.ToLowerInvariant().Trim();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(ScGimpSubscriber obj)
        {
            if (obj != null && !string.IsNullOrWhiteSpace(obj.Email))
            {
                return obj.Email.ToLowerInvariant().Trim().GetHashCode();
            }
            else
            {
                return 0;
            }
        }
    }
}
