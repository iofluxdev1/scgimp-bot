using Microsoft.AspNetCore.Mvc;

namespace StarCitizen.Gimp.Web.Models
{
    public class BadGimpViewModel
    {
        public string Message { get; set; }

        [FromForm(Name = "g-recaptcha-response")]
        public string RecaptchaResponse { get; set; }
    }
}
