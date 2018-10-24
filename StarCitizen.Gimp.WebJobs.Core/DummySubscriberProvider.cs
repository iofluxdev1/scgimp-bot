using StarCitizen.Gimp.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StarCitizen.Gimp.WebJobs.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class DummySubscriberProvider : ISubscriberProvider, IDiscordWebhookProvider, INotificationLogProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public DummySubscriberProvider()
        {
            
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// 
        /// </summary>
        /// <param name="discordWebhook"></param>
        /// <returns></returns>
        public async Task DeleteAsync(ScGimpDiscordWebhook discordWebhook)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotImplementedException();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ScGimpDiscordWebhook>> GetDiscordWebhooksAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return new ScGimpDiscordWebhook[] 
            {
                new ScGimpDiscordWebhook()
                {
                    Url = "https://discordapp.com/api/webhooks/429370121493151756/vpoJuRKmwBFNrktG5y8j1v_ln9VyZlCGitTTavN_2VvDmDzS7mPZIshroiqyVhCQo5CU",
                    Id = 1L
                }
            };
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ScGimpSubscriber>> GetSubscribersAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return new ScGimpSubscriber[] 
            {
                new ScGimpSubscriber()
                {
                    Email = "youremail@hotmail.com",
                    Id = 1L
                }
            };
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// 
        /// </summary>
        /// <param name="discordWebhook"></param>
        /// <returns></returns>
        public async Task RegisterAsync(ScGimpDiscordWebhook discordWebhook)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotImplementedException();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        public async Task SubscribeAsync(ScGimpSubscriber subscriber)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotImplementedException();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        public async Task UnsubscribeAsync(ScGimpSubscriber subscriber)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotImplementedException();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logEntry"></param>
        /// <returns></returns>
        public async Task WriteAsync(ScGimpNotification logEntry)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            
        }
    }
}
