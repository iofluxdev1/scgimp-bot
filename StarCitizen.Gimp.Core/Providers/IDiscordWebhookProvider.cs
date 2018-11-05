using System.Collections.Generic;
using System.Threading.Tasks;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDiscordWebhookProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ScGimpDiscordWebhook>> GetDiscordWebhooksAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="discordWebhook"></param>
        /// <returns></returns>
        Task RegisterAsync(ScGimpDiscordWebhook discordWebhook);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="discordWebhook"></param>
        /// <returns></returns>
        Task DeleteAsync(ScGimpDiscordWebhook discordWebhook);
    }
}