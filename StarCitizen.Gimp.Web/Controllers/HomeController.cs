using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StarCitizen.Gimp.Core;
using StarCitizen.Gimp.Data;
using StarCitizen.Gimp.Web.Models;

namespace StarCitizen.Gimp.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ScGimpContext _context;

        public HomeController(ScGimpContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "";

            SubscribeViewModel model = new SubscribeViewModel
            {
                Stats = await GetStats()
            };

            return View(model);
        }

        public async Task<IActionResult> Subscribe()
        {
            ViewData["Title"] = "Subscribe for automatic ship sale and comm-link notifications";

            SubscribeViewModel model = new SubscribeViewModel
            {
                Stats = await GetStats()
            };

            return View("Index", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SubscribeViewModel model)
        {
            return await HandleSubscription("Index", model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(SubscribeViewModel model)
        {
            return await HandleSubscription("Index", model);
        }

        private async Task<IActionResult> HandleSubscription(string viewName, SubscribeViewModel model)
        {
            try
            {
                model.Message = string.Empty;

                bool recaptchaResult = await VerifyGoogleRecaptcha(model.RecaptchaResponse);

                if (!recaptchaResult)
                {
                    model.Message = "Recaptcha validation failed.";

                    return View(model);
                }

                if (ModelState.IsValid)
                {
                    // Find existing subscriber.
                    Subscriber dbSubscriber = await
                    (
                        from s in _context.Subscribers
                        where
                            string.Compare(s.Email, model.Email, true) == 0
                        select s
                    ).FirstOrDefaultAsync();

                    // Was existing found?
                    if (dbSubscriber != null)
                    {
                        // Existing found, is it subscribed?
                        if (dbSubscriber.DeletedAt != null)
                        {
                            // Update unsubscribed.
                            dbSubscriber.DeletedAt = null;

                            // Save changes.
                            await _context.SaveChangesAsync();
                        }
                        // Else nothing, already subscribed.

                        model.Message = "You have been successfully subscribed.";
                    }
                    else
                    {
                        // Add new subscriber
                        await _context.Subscribers.AddAsync
                        (
                            new Subscriber()
                            {
                                CreatedAt = DateTimeOffset.Now,
                                DeletedAt = null,
                                Email = model.Email.Length > 250 ?
                                    model.Email.Substring(0, 250) :
                                    model.Email,
                                SubscriberId = 0,
                                UpdatedAt = DateTimeOffset.Now
                            }
                        );

                        await _context.SaveChangesAsync();

                        SendWelcomeEmail(model.Email);

                        model.Message = "You have been successfully subscribed.";
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO log
                model.Message = "An error has occured. Please try again later.";
                Trace.WriteLine(ex);
            }

            model.Stats = await GetStats();

            return View(viewName, model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unsubscribe(UnsubscribeViewModel model)
        {
            model.Message = string.Empty;

            try
            {
                if (ModelState.IsValid)
                {
                    Subscriber dbSubscriber = await
                       (
                           from s in _context.Subscribers
                           where
                               string.Compare(s.Email, model.Email, true) == 0
                           select s
                       ).FirstOrDefaultAsync();

                    if (dbSubscriber != null &&
                        dbSubscriber.DeletedAt == null)
                    {
                        dbSubscriber.DeletedAt = DateTimeOffset.Now;

                        await _context.SaveChangesAsync();
                    }

                    model.Message = "You have been successfully unsubscribed.";
                }
            }
            catch (Exception ex)
            {
                // TODO log
                model.Message = "An error has occured. Please try again later.";
                Trace.WriteLine(ex);
            }

            return View(model);
        }

        public IActionResult Unsubscribe()
        {
            return View(new UnsubscribeViewModel());
        }

        public async Task<IActionResult> DiscordWebhooks()
        {
            AddDiscordWebhookViewModel model = new AddDiscordWebhookViewModel
            {
                Stats = await GetStats()
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DiscordWebhooks(AddDiscordWebhookViewModel model)
        {
            try
            {
                string failedTestMessage = "Failed to register Discord Webhook. Please make sure you have created the Discord Webhook on your Discord server and that the provided URL is valid.";
                string successMessage = "Your Discord Webhook has been successfully registered.";

                model.Message = string.Empty;

                bool recaptchaResult = await VerifyGoogleRecaptcha(model.RecaptchaResponse);

                if (!recaptchaResult)
                {
                    model.Message = "Recaptcha validation failed.";

                    return View(model);
                }

                if (ModelState.IsValid)
                {
                    // Find existing discord webhook.
                    DiscordWebhook dbDiscordWebhook = await
                    (
                        from s in _context.DiscordWebhooks
                        where
                            string.Compare(s.Url, model.Url, true) == 0
                        select s
                    ).FirstOrDefaultAsync();

                    // Was existing found?
                    if (dbDiscordWebhook != null)
                    {
                        // Existing found, is it registered?
                        if (dbDiscordWebhook.DeletedAt != null)
                        {
                            // Test discord webhook
                            bool isDiscordWebhookValid = await ValidateDiscordWebhook(model.Url);

                            if (isDiscordWebhookValid)
                            {
                                // Update deregistered.
                                dbDiscordWebhook.DeletedAt = null;

                                // Save changes.
                                await _context.SaveChangesAsync();

                                model.Message = successMessage;
                            }
                            else
                            {
                                model.Message = failedTestMessage;
                            }
                        }
                        else
                        {
                            // Else nothing, already registered.

                            model.Message = successMessage;
                        }
                        
                    }
                    else
                    {
                        // Test discord webhook
                        bool isDiscordWebhookValid = await ValidateDiscordWebhook(model.Url);

                        if (isDiscordWebhookValid)
                        {
                            // Add new discord webhook
                            await _context.DiscordWebhooks.AddAsync
                            (
                                new DiscordWebhook()
                                {
                                    CreatedAt = DateTimeOffset.Now,
                                    DeletedAt = null,
                                    Url = model.Url.Length > 250 ?
                                        model.Url.Substring(0, 250) :
                                        model.Url,
                                    DiscordWebhookId = 0,
                                    UpdatedAt = DateTimeOffset.Now
                                }
                            );

                            await _context.SaveChangesAsync();

                            model.Message = successMessage;
                        }
                        else
                        {
                            model.Message = failedTestMessage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO log
                model.Message = "An error has occured. Please try again later.";
                Trace.WriteLine(ex);
            }

            model.Stats = await GetStats();

            return View(model);
        }

        private async Task<ScGimpStatsViewModel> GetStats()
        {
            ScGimpStatsViewModel model = new ScGimpStatsViewModel();

            try
            {
                model.TotalActiveDiscordWebhookSubscribers = await _context.DiscordWebhooks.CountAsync(s => s.DeletedAt == null);
                model.TotalActiveEmailSubscribers = await _context.Subscribers.CountAsync(s => s.DeletedAt == null);

                Notification[] notifications = await _context.Notifications
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(10)
                    .ToArrayAsync();

                model.NotificationLog = notifications
                    .Select
                    (
                        n => new NotificationViewModel()
                        {
                            Body = n.Body,
                            CreatedAt = n.CreatedAt,
                            Medium = n.Medium,
                            NotificationType = n.NotificationType,
                            TotalRecipients = n.Recipients.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).Length
                        }
                    ).OrderByDescending(n => n.CreatedAt)
                    .ToArray();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(new Exception("Failed to get SC Gimp stats. Please see inner exception for more information.", ex));
            }

            return model;
        }

        private async Task<bool> ValidateDiscordWebhook(string url)
        {
            bool isValid = false;

            if (string.IsNullOrWhiteSpace(url))
            {
                return isValid;
            }

            string postUrl = $"{url}?wait=true";

            try
            {
                ServicePoint servicePoint = ServicePointManager.FindServicePoint(new Uri(postUrl));
                int timeout = 60 * 1000; // 1 minute

                servicePoint.ConnectionLeaseTimeout = timeout;

                using (HttpClient client = new HttpClient())
                {
                    string content = "Greetings Citizen,\\n\\nYour Discord server has successfully been integrated with the Star Citizen Gimp bot.\\n\\nYou will now receive notifications in this channel from the bot.\\n\\nThis is a free sevice, but it does cost something to host the services on the cloud, as such any donations https://scgimp.com#donations are greatly appreciated.\\n\\nYou can go to http://scgimp.com/home/deletediscordwebhook to deregister your Discord Webhook at any time.";
                    string jsonPayload = $"{{ \"content\": \"{content}\" }}";
                   
                    using (HttpResponseMessage response = await client.PostAsync(postUrl, new StringContent(jsonPayload, Encoding.UTF8, "application/json")))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            isValid = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO log
                Trace.WriteLine(ex);
            }

            return isValid;
        }

        public IActionResult DeleteDiscordWebhook()
        {
            return View(new DeleteDiscordWebhookViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDiscordWebhook(DeleteDiscordWebhookViewModel model)
        {
            model.Message = string.Empty;

            try
            {
                if (ModelState.IsValid)
                {
                    DiscordWebhook dbDiscordWebhook = await
                       (
                           from s in _context.DiscordWebhooks
                           where
                               string.Compare(s.Url, model.Url, true) == 0
                           select s
                       ).FirstOrDefaultAsync();

                    if (dbDiscordWebhook != null &&
                        dbDiscordWebhook.DeletedAt == null)
                    {
                        dbDiscordWebhook.DeletedAt = DateTimeOffset.Now;

                        await _context.SaveChangesAsync();
                    }

                    model.Message = "Your Discord Webhook has been successfully deregistered.";
                }
            }
            catch (Exception ex)
            {
                // TODO log
                model.Message = "An error has occured. Please try again later.";
                Trace.WriteLine(ex);
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private SmtpClient CreateSmtpClient(IConfigurationRoot configuration)
        {
            SmtpClient client = new SmtpClient(configuration["Host"])
            {
                Port = int.Parse(configuration["Port"]),
                Credentials = new NetworkCredential(configuration["Username"], configuration["Password"])
            };

            return client;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        private void SendWelcomeEmail(string email)
        {
            try
            {
                IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

                IConfigurationRoot configuration = configBuilder.Build();

                using (SmtpClient client = CreateSmtpClient(configuration))
                {
                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(configuration["From"], "Star Citizen Gimp"),
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(new MailAddress(email));

                    mailMessage.Body = FormatWelcomeBody();
                    mailMessage.Subject = "Star Citizen Gimp: Subscription added.";

                    client.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // TODO log
                Trace.WriteLine(ex);
            }
        }

        private string FormatWelcomeBody()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("<div>");
            builder.AppendLine("<p>Greetings Citizen,</p>");
            builder.AppendLine("<p>You have been subscribed to the Star Citizen Gimp bot notification service.</p>");
            builder.AppendLine("<p>This is a free sevice, but it does cost something to host the services on the cloud, as such any <a href=\"https://scgimp.com#donations\">donations</a> are greatly appreciated.</p>");
            builder.AppendLine("<p>If you wish to unsubscribe from these notifications at a later date you can <a href=\"https://scgimp.com/home/unsubscribe\">unsubscribe</a> by clicking <a href=\"https://scgimp.com/home/unsubscribe\">here</a>.</p>");
            builder.AppendLine("<p>Happy gaming and see you in the verse!</p>");
            builder.AppendLine("</div>");

            return builder.ToString();
        }

        private async Task<bool> VerifyGoogleRecaptcha(string recaptchaResponse)
        {
            try
            {
                IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = configBuilder.Build();
                string remoteIp = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                string url = "https://www.google.com/recaptcha/api/siteverify";

                HttpClient client = new HttpClient();
                FormUrlEncodedContent content = new FormUrlEncodedContent(new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("secret", configuration["RecaptchaKey"]),
                    new KeyValuePair<string, string>("response", recaptchaResponse),
                    new KeyValuePair<string, string>("remoteip", remoteIp)
                });
                
                using (HttpResponseMessage response = await client.PostAsync(url, content))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"HTTP post request failed for URL: {url} with status {response.StatusCode.ToString()}");
                    }

                    // Read content.
                    string postResponseContent = await response.Content.ReadAsStringAsync();

                    dynamic jsonContent = Newtonsoft.Json.JsonConvert.DeserializeObject(postResponseContent);

                    return jsonContent.success;
                }
            }
            catch (Exception ex)
            {
                //TODO log
                Trace.WriteLine(ex);
                return false;
            }
        }
    }
}
