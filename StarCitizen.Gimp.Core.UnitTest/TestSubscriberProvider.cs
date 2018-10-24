using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StarCitizen.Gimp.Core.UnitTest
{
    public class TestSubscriberProvider : ISubscriberProvider, IDiscordWebhookProvider, INotificationLogProvider
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IEnumerable<ScGimpDiscordWebhook>> GetDiscordWebhooksAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return new ScGimpDiscordWebhook[]
            {
                new ScGimpDiscordWebhook() { Url = "https://discordapp.com/api/webhooks/429370121493151756/vpoJuRKmwBFNrktG5y8j1v_ln9VyZlCGitTTavN_2VvDmDzS7mPZIshroiqyVhCQo5CU" },
                new ScGimpDiscordWebhook() { Url = "invalid garbage" }
            };
        }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IEnumerable<ScGimpSubscriber>> GetSubscribersAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return new ScGimpSubscriber[]
            {
                new ScGimpSubscriber() { Email = "aspruyt@hotmail.co.uk" },
                new ScGimpSubscriber() { Email = "aspruyt@hotmail.co" },
                new ScGimpSubscriber() { Email = "aspruyt@hotmail" },
                new ScGimpSubscriber() { Email = "@hotmail.co.za" },
                new ScGimpSubscriber() { Email = "hotmail.co.za" },
                new ScGimpSubscriber() { Email = "co.za" },
                new ScGimpSubscriber() { Email = "hotmail.co" },
                new ScGimpSubscriber() { Email = "hotmail" },
                new ScGimpSubscriber() { Email = "@hotmail" }
            };
        }

        public Task DeleteAsync(ScGimpDiscordWebhook discordWebhook)
        {
            throw new NotImplementedException();
        }

        public Task RegisterAsync(ScGimpDiscordWebhook discordWebhook)
        {
            throw new NotImplementedException();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task SubscribeAsync(ScGimpSubscriber subscriber)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotImplementedException();
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
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
