using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace StarCitizen.Gimp.Web.Models
{
    public class UnsubscribeViewModel
    {
        [Required]
        [MaxLength(250)]
        [EmailAddress]
        [RegularExpression(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$")]
        public string Email { get; set; }

        public string Message { get; set; }

        [FromForm(Name = "g-recaptcha-response")]
        public string RecaptchaResponse { get; set; }
    }
}