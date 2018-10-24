using System;

namespace StarCitizen.Gimp.Core
{
    public class DiscordWebhookDeletedException : Exception
    {
        public DiscordWebhookDeletedException()
        {
        }

        public DiscordWebhookDeletedException(string message)
            : base(message)
        {
        }

        public DiscordWebhookDeletedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
