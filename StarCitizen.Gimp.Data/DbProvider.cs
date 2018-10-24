using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using StarCitizen.Gimp.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarCitizen.Gimp.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class DbProvider : ISubscriberProvider, IDiscordWebhookProvider, INotificationLogProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public DbProvider()
        {

        }

        public async Task<IEnumerable<ScGimpDiscordWebhook>> GetDiscordWebhooksAsync()
        {
            using (ScGimpContext db = new ScGimpContext())
            {
                IQueryable<ScGimpDiscordWebhook> discordWebhooks =
                (
                    from s in db.DiscordWebhooks
                    where
                        s.DeletedAt == null
                    select new ScGimpDiscordWebhook() { Url = s.Url }
                );

                return await discordWebhooks.ToArrayAsync();
            }
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
        /// <param name="discordWebhook"></param>
        /// <returns></returns>
        public async Task RegisterAsync(ScGimpDiscordWebhook discordWebhook)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<ScGimpSubscriber>> GetSubscribersAsync()
        {
            using (ScGimpContext db = new ScGimpContext())
            {
                IQueryable<ScGimpSubscriber> subscribers =
                (
                    from s in db.Subscribers
                    where
                        s.DeletedAt == null
                    select new ScGimpSubscriber() { Email = s.Email }
                );

                return await subscribers.ToArrayAsync();
            }
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

        public async Task WriteAsync(ScGimpNotification logEntry)
        {
            using (ScGimpContext db = new ScGimpContext())
            {
                // Add new notification
                await db.Notifications.AddAsync
                (
                    new Notification()
                    {
                        Body = logEntry.Body,
                        CreatedAt = DateTimeOffset.Now,
                        Items = logEntry.Items,
                        NotificationId = 0,
                        NotificationType = logEntry.NotificationType,
                        Recipients = logEntry.Recipients
                    }
                );

                await db.SaveChangesAsync();
            }
        }
    }
}
