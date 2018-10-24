using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace StarCitizen.Gimp.Data
{
    public class DiscordWebhook
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long DiscordWebhookId { get; set; }

        [Required]
        [MaxLength(250)]
        [RegularExpression(@"^https://discordapp.com/api/webhooks/[0-9]*/\S*$")]
        public string Url { get; set; }

        [Required]
        public DateTimeOffset CreatedAt { get; set; }

        [Required]
        public DateTimeOffset UpdatedAt { get; set; }

        public DateTimeOffset? DeletedAt { get; set; }
    }
}
