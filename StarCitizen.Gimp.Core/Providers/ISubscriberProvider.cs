using System.Collections.Generic;
using System.Threading.Tasks;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISubscriberProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ScGimpSubscriber>> GetSubscribersAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        Task SubscribeAsync(ScGimpSubscriber subscriber);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        Task UnsubscribeAsync(ScGimpSubscriber subscriber);
    }
}