using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace StarCitizen.Gimp.Web.Models
{
    public class AddDiscordWebhookViewModel
    {
        [Required]
        [MaxLength(250)]
        [RegularExpression(@"^https://discordapp.com/api/webhooks/[0-9]*/\S*$")]
        public string Url { get; set; }

        public string Message { get; set; }

        [FromForm(Name = "g-recaptcha-response")]
        public string RecaptchaResponse { get; set; }

        public ScGimpStatsViewModel Stats { get; set; }
    }
}
