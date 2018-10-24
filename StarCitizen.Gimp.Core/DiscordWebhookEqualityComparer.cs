using System;
using System.Collections.Generic;
using System.Text;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class DiscordWebhookEqualityComparer : IEqualityComparer<ScGimpDiscordWebhook>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(ScGimpDiscordWebhook x, ScGimpDiscordWebhook y)
        {
            return x?.Url?.ToLowerInvariant().Trim() == y?.Url?.ToLowerInvariant().Trim();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(ScGimpDiscordWebhook obj)
        {
            return obj.Url.GetHashCode();
        }
    }
}
