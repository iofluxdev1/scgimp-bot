using Microsoft.EntityFrameworkCore;
using StarCitizen.Gimp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    select new ScGimpDiscordWebhook()
                    {
                        Url = s.Url,
                        Id = s.DiscordWebhookId
                    }
                );

                return await discordWebhooks.ToArrayAsync();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="discordWebhook"></param>
        /// <returns></returns>
        public async Task DeleteAsync(ScGimpDiscordWebhook discordWebhook)
        {
            using (ScGimpContext db = new ScGimpContext())
            {
                DiscordWebhook dbDiscordWebhook = await
                (
                    from s in db.DiscordWebhooks
                    where
                        s.DiscordWebhookId == discordWebhook.Id
                    select s
                ).FirstOrDefaultAsync();

                if (dbDiscordWebhook != null)
                {
                    dbDiscordWebhook.DeletedAt = DateTimeOffset.Now;

                    await db.SaveChangesAsync();
                }
            }
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
                    select new ScGimpSubscriber()
                    {
                        Email = s.Email,
                        Id = s.SubscriberId
                    }
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
                        Body = logEntry.Body.Left(4000),
                        CreatedAt = DateTimeOffset.Now,
                        Items = logEntry.Items.Left(4000),
                        NotificationId = 0,
                        NotificationType = logEntry.NotificationType.Left(50),
                        Recipients = logEntry.Recipients.Left(4000),
                        Medium = logEntry.Medium.Left(50)
                    }
                );

                await db.SaveChangesAsync();
            }
        }
    }
}
