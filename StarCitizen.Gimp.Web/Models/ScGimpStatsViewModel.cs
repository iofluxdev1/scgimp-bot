using System;
using System.Collections.Generic;

namespace StarCitizen.Gimp.Web.Models
{
    public class ScGimpStatsViewModel
    {
        public int TotalActiveEmailSubscribers { get; set; }

        public int TotalActiveDiscordWebhookSubscribers { get; set; }

        public IEnumerable<NotificationViewModel> NotificationLog { get; set; }
    }

    public class NotificationViewModel
    {
        public int TotalRecipients { get; set; }
        public string Body { get; set; }
        public string NotificationType { get; set; }
        public string Medium { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
