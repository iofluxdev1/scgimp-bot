using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using GoogleMeasurementProtocol;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using StarCitizen.Gimp.Data;
using StarCitizen.Gimp.Web.Models;

namespace StarCitizen.Gimp.Web.Controllers
{
    [ResponseCache(CacheProfileName = "Never")]
    public class HomeController : Controller
    {
        private static readonly string __bgrKey = "BadGimpReports";
        private static readonly string __tadwsKey = "TotalActiveDiscordWebhookSubscribers";
        private static readonly string __taesKey = "TotalActiveEmailSubscribers";
        private static readonly string __nhKey = "NotificationHistory";
        private static readonly TimeSpan __defaultCacheLifetime = TimeSpan.FromDays(7);
        private static readonly TimeSpan __notificationLogCacheLifetime = TimeSpan.FromMinutes(5);
        
        private readonly ScGimpContext _context;

        private readonly IMemoryCache _cache;

        private readonly IHttpContextAccessor _accessor;

        public HomeController
        (
            ScGimpContext context, 
            IMemoryCache memoryCache, 
            IHttpContextAccessor accessor
        )
        {
            _context = context;
            _cache = memoryCache;
            _accessor = accessor;
        }

        public async Task<IActionResult> Index()
        {
            SubscribeViewModel model = new SubscribeViewModel
            {
                Stats = await GetStats()
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SubscribeViewModel model)
        {
            return View(await HandleSubscription(model));
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult ReferralCode()
        {
            return View();
        }

        public async Task<IActionResult> Subscribe()
        {
            SubscribeViewModel model = new SubscribeViewModel
            {
                Stats = await GetStats()
            };

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe(SubscribeViewModel model)
        {
            return View(await HandleSubscription(model));
        }

        public IActionResult BadGimp()
        {
            return View(new BadGimpViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BadGimp(BadGimpViewModel model)
        {
            try
            {
                model.Message = string.Empty;

                bool recaptchaResult = await VerifyGoogleRecaptcha(model.RecaptchaResponse);

                if (!recaptchaResult)
                {
                    model.Message = "<div class=\"alert alert-danger\">Recaptcha validation failed.</div>";

                    return View(model);
                }

                if (ModelState.IsValid)
                {
                    if (_cache.TryGetValue(__bgrKey, out BadGimpReports bgr))
                    {
                        string ip = GetRemoteIpAddress();

                        if (!bgr.Ips.Contains(ip))
                        {
                            ScGimpWebConfig config = new ScGimpWebConfig();

                            bgr.Ips.Add(ip);

                            bgr.Count++;

                            _cache.Set
                            (
                                __bgrKey, 
                                bgr, 
                                DateTimeOffset.Now.AddMinutes(30d)
                            );

                            if (bgr.Count >= 5)
                            {

                                KuduWebJobService kudu = new KuduWebJobService(config.KuduUserName, config.KuduPassword, config.AzureWebsiteName);

                                string rssWebJobStatus = kudu.GetWebJobStatus(config.RssWebJobName).ToLowerInvariant();
                                string storeWebJobStatus = kudu.GetWebJobStatus(config.StoreWebJobName).ToLowerInvariant();
                                string spectrumWebJobStatus = kudu.GetWebJobStatus(config.SpectrumWebJobName).ToLowerInvariant();

                                bool stoppedABadGimp = false;

                                if (rssWebJobStatus != "stopped" &&
                                    rssWebJobStatus != "aborted" &&
                                    rssWebJobStatus != "abandoned" &&
                                    rssWebJobStatus != "failure")
                                {
                                    kudu.StopWebJob(config.RssWebJobName);

                                    stoppedABadGimp = true;
                                }

                                if (storeWebJobStatus != "stopped" &&
                                    storeWebJobStatus != "aborted" &&
                                    storeWebJobStatus != "abandoned" &&
                                    storeWebJobStatus != "failure")
                                {
                                    kudu.StopWebJob(config.StoreWebJobName);

                                    stoppedABadGimp = true;
                                }

                                if (spectrumWebJobStatus != "stopped" &&
                                    spectrumWebJobStatus != "aborted" &&
                                    spectrumWebJobStatus != "abandoned" &&
                                    spectrumWebJobStatus != "failure")
                                {
                                    kudu.StopWebJob(config.SpectrumWebJobName);

                                    stoppedABadGimp = true;
                                }

                                if (stoppedABadGimp)
                                {
                                    model.Message = "<div class=\"alert alert-success\">Thank you for letting us know that the gimp is out of hand. The Gimp has been stopped.<span>";

                                    SendBadGimpEmail(model, bgr, config);
                                }
                                else
                                {
                                    model.Message = "<span class=\"text-info\">Thank you for letting us know that the gimp was out of hand. Luckily the Gimp has already been stopped a while ago.</div>";
                                }
                            }
                            else
                            {
                                model.Message = $"<div class=\"alert alert-success\">Thank you for letting us know that the gimp is out of hand. Another {5 - bgr.Count} reports are required to stop the gimp.</div>";

                                SendBadGimpEmail(model, bgr, config);
                            }
                        }
                        else
                        {
                            model.Message = "<span class=\"text-info\">Thank you for letting us know that the gimp is out of hand. You have already submitted a report in the last 30 minutes.</div>";
                        }
                    }
                    else
                    {
                        _cache.Set
                        (
                            __bgrKey, 
                            new BadGimpReports()
                            {
                                Count = 1,
                                Ips = new List<string>() { GetRemoteIpAddress() }
                            }, 
                            DateTimeOffset.Now.AddMinutes(30d)
                        );

                        model.Message = "<div class=\"alert alert-success\">Thank you for letting us know that the gimp is out of hand. Your report is the first one in the last 30 minutes. Another 4 reports are required to stop the gimp and notify the administrator.</div>";
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO log
                model.Message = "<div class=\"alert alert-danger\">An error has occured. Please try again later.</div>";
                Trace.WriteLine(ex);
            }

            return View(model);
        }

        public IActionResult Unsubscribe()
        {
            return View(new UnsubscribeViewModel());
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

                        // Create audit entry.
                        await _context.SubscriberAudits.AddAsync
                        (
                            new SubscriberAudit()
                            {
                                Action = "Unsubscribe".Left(200),
                                CreatedAt = DateTimeOffset.Now,
                                FormName = "Email".Left(50),
                                FormVersion = "1.0.0".Left(16),
                                Headers = GetHeaders().Left(4000),
                                IpAddress = GetRemoteIpAddress().Left(39),
                                SubscriberId = dbSubscriber.SubscriberId
                            }
                        );

                        await _context.SaveChangesAsync();

                        if (_cache.TryGetValue(__taesKey, out int value))
                        {
                            _cache.Remove(__taesKey);
                        }

                        await SendGoogleAnalyticsConversion
                        (
                            "email",
                            "unsubscribe",
                            "na",
                            model.Email.ToLowerInvariant().GetHashCode().ToString()
                        );
                    }

                    model.Message = "<div class=\"alert alert-success\">You have been successfully unsubscribed.</div>";
                }
            }
            catch (Exception ex)
            {
                // TODO log
                model.Message = "<div class=\"alert alert-danger\">An error has occured. Please try again later.</div>";
                Trace.WriteLine(ex);
            }

            return View(model);
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
                string failedTestMessage = "<div class=\"alert alert-danger\">Failed to register Discord Webhook. Please make sure you have created the Discord Webhook on your Discord server and that the provided URL is valid.</div>";
                string successMessage = "<div class=\"alert alert-success\">Your Discord Webhook has been successfully registered.</div>";

                model.Message = string.Empty;

                bool recaptchaResult = await VerifyGoogleRecaptcha(model.RecaptchaResponse);

                if (!recaptchaResult)
                {
                    model.Message = "<div class=\"alert alert-danger\">Recaptcha validation failed.</div>";

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

                                if (_cache.TryGetValue(__tadwsKey, out int value))
                                {
                                    _cache.Remove(__tadwsKey);
                                }

                                await SendGoogleAnalyticsConversion
                                (
                                    "discord-webhook",
                                    "reregister",
                                    "na",
                                    model.Url.ToLowerInvariant().GetHashCode().ToString()
                                );

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
                                    Url = model.Url.Left(250),
                                    DiscordWebhookId = 0,
                                    UpdatedAt = DateTimeOffset.Now
                                }
                            );

                            await _context.SaveChangesAsync();

                            if (_cache.TryGetValue(__tadwsKey, out int value))
                            {
                                _cache.Remove(__tadwsKey);
                            }

                            await SendGoogleAnalyticsConversion
                            (
                                "discord-webhook",
                                "register",
                                "na",
                                model.Url.ToLowerInvariant().GetHashCode().ToString()
                            );

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
                model.Message = "<div class=\"alert alert-danger\">An error has occured. Please try again later.</div>";
                Trace.WriteLine(ex);
            }

            model.Stats = await GetStats();

            return View(model);
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

                        if (_cache.TryGetValue(__tadwsKey, out int value))
                        {
                            _cache.Remove(__tadwsKey);
                        }

                        await SendGoogleAnalyticsConversion
                        (
                            "discord-webhook",
                            "deregister",
                            "na",
                            model.Url.ToLowerInvariant().GetHashCode().ToString()
                        );
                    }

                    model.Message = "<div class=\"alert alert-success\">Your Discord Webhook has been successfully deregistered.</div>";
                }
            }
            catch (Exception ex)
            {
                // TODO log
                model.Message = "<div class=\"alert alert-danger\">An error has occured. Please try again later.</div>";
                Trace.WriteLine(ex);
            }

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        private string GetHeaders()
        {
            string headers = string.Join(";", Request.Headers.Select(h => $"{h.Key}:{h.Value}"));

            return headers.Left(4000);
        }

        private async Task SendGoogleAnalyticsConversion(string category, string action, string label, string userId)
        {
            try
            {
                GoogleAnalyticsRequestFactory factory = new GoogleAnalyticsRequestFactory("UA-65901374-3");

                // Create a PageView request by specifying request type
                var request = factory.CreateRequest(HitTypes.Event);

                //Add parameters to your request, each parameter has a corresponding class which has name = parameter name from google reference docs
                request.Parameters.Add(new GoogleMeasurementProtocol.Parameters.EventTracking.EventCategory(category));
                request.Parameters.Add(new GoogleMeasurementProtocol.Parameters.EventTracking.EventAction(action));
                request.Parameters.Add(new GoogleMeasurementProtocol.Parameters.EventTracking.EventLabel(label));

                await request.PostAsync(new GoogleMeasurementProtocol.Parameters.User.UserId(userId));
            }
            catch (Exception ex)
            {
                // TODO log
                Trace.WriteLine(ex);
            }
        }

        private async Task<SubscribeViewModel> HandleSubscription(SubscribeViewModel model)
        {
            model.Stats = await GetStats();

            try
            {
                if (model.Email.ToLowerInvariant().Contains("web.de") ||
                    model.Email.ToLowerInvariant().Contains("gmx.de") ||
                    model.Email.ToLowerInvariant().Contains("streber24.de"))
                {
                    model.Message = "<div class=\"alert alert-danger\">Your email provider (web.de, gmx.de and streber24.de) has blacklisted this IP address for no reason and I can't be arsed to contact them and convince them this is not spam. As such you will have to register with another address if you want to subscribe to this service or contact them yourself and get it delisted and in turn contact @fluxxie via spectrum.</div>";

                    return model;
                }
                else if (model.Email.ToLowerInvariant().Contains("francesco_uni@hotmail.it"))
                {
                    model.Message = "<div class=\"alert alert-danger\">You have been banned from using this service due to being too lazy to unsubscribe and reporting notifications as spam for which you signed up yourself.</div>";

                    return model;
                }

                model.Message = string.Empty;

                bool recaptchaResult = await VerifyGoogleRecaptcha(model.RecaptchaResponse);

                if (!recaptchaResult)
                {
                    model.Message = "<div class=\"alert alert-danger\">Recaptcha validation failed.</div>";

                    return model;
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

                            // Create audit entry.
                            await _context.SubscriberAudits.AddAsync
                            (
                                new SubscriberAudit()
                                {
                                    Action = "Resubscribe".Left(200),
                                    CreatedAt = DateTimeOffset.Now,
                                    FormName = "Email".Left(50),
                                    FormVersion = "1.0.0".Left(16),
                                    Headers = GetHeaders().Left(4000),
                                    IpAddress = GetRemoteIpAddress().Left(39),
                                    SubscriberId = dbSubscriber.SubscriberId
                                }
                            );

                            // Save changes.
                            await _context.SaveChangesAsync();

                            if (_cache.TryGetValue(__taesKey, out int value))
                            {
                                _cache.Remove(__taesKey);
                            }

                            await SendGoogleAnalyticsConversion
                            (
                                "email",
                                "resubscribe",
                                "na",
                                model.Email.ToLowerInvariant().GetHashCode().ToString()
                            );
                        }
                        // Else nothing, already subscribed.

                        model.Message = "<div class=\"alert alert-success\">You have been successfully subscribed.</div>";
                    }
                    else
                    {
                        dbSubscriber = new Subscriber()
                        {
                            CreatedAt = DateTimeOffset.Now,
                            DeletedAt = null,
                            Email = model.Email.Left(250),
                            SubscriberId = 0,
                            UpdatedAt = DateTimeOffset.Now
                        };

                        // Add new subscriber
                        await _context.Subscribers.AddAsync(dbSubscriber);

                        await _context.SaveChangesAsync();

                        // Create audit entry.
                        await _context.SubscriberAudits.AddAsync
                        (
                            new SubscriberAudit()
                            {
                                Action = "Subscribe".Left(200),
                                CreatedAt = DateTimeOffset.Now,
                                FormName = "Email".Left(50),
                                FormVersion = "1.0.0".Left(16),
                                Headers = GetHeaders().Left(4000),
                                IpAddress = GetRemoteIpAddress().Left(39),
                                SubscriberId = dbSubscriber.SubscriberId
                            }
                        );

                        await _context.SaveChangesAsync();

                        if (_cache.TryGetValue(__taesKey, out int value))
                        {
                            _cache.Remove(__taesKey);
                        }

                        SendWelcomeEmail(model.Email);

                        await SendGoogleAnalyticsConversion
                        (
                            "email", 
                            "subscribe", 
                            "na", 
                            model.Email.ToLowerInvariant().GetHashCode().ToString()
                        );

                        model.Message = "<div class=\"alert alert-success\">You have been successfully subscribed.</div>";
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO log
                model.Message = "<div class=\"alert alert-danger\">An error has occured. Please try again later.</div>";
                Trace.WriteLine(ex);
            }

            return model;
        }

        private async Task<ScGimpStatsViewModel> GetStats()
        {
            ScGimpStatsViewModel model = new ScGimpStatsViewModel();

            try
            {
                if (_cache.TryGetValue(__tadwsKey, out int value1))
                {
                    model.TotalActiveDiscordWebhookSubscribers = value1;
                }
                else
                {
                    model.TotalActiveDiscordWebhookSubscribers = await _context.DiscordWebhooks.CountAsync(s => s.DeletedAt == null);

                    _cache.Set
                    (
                        __tadwsKey, 
                        model.TotalActiveDiscordWebhookSubscribers, 
                        __defaultCacheLifetime
                    );
                }

                if (_cache.TryGetValue(__taesKey, out int value2))
                {
                    model.TotalActiveEmailSubscribers = value2;
                }
                else
                {
                    model.TotalActiveEmailSubscribers = await _context.Subscribers.CountAsync(s => s.DeletedAt == null);

                    _cache.Set
                    (
                        __taesKey, 
                        model.TotalActiveEmailSubscribers, 
                        __defaultCacheLifetime
                    );
                }

                if (_cache.TryGetValue(__nhKey, out NotificationViewModel[] value3))
                {
                    model.NotificationLog = value3;
                }
                else
                {
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

                    _cache.Set
                    (
                        __nhKey, 
                        model.NotificationLog, 
                        __notificationLogCacheLifetime
                    );
                }


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
                    string content = "Greetings Citizen,\\n\\nYour Discord server has successfully been integrated with the Star Citizen Gimp bot.\\n\\nYou will now receive notifications in this channel from the bot.\\n\\nThis is a free sevice, but it does cost something to host the services on the cloud (about $80 USD per month), as such any donations https://scgimp.com#donations are greatly appreciated.\\n\\nYou can go to http://scgimp.com/home/deletediscordwebhook to deregister your Discord Webhook at any time.\\n\\nShould the gimp misbehave and go into a spam loop please go report it at http://scgimp.com/home/badgimp to stop the bot and notify an administrator.";
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private SmtpClient CreateSmtpClient(ScGimpWebConfig config)
        {
            SmtpClient client = new SmtpClient(config.Options.Host)
            {
                Port = config.Options.Port ?? 587,
                Credentials = new NetworkCredential(config.Options.Username, config.Options.Password)
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
                ScGimpWebConfig config = new ScGimpWebConfig();

                using (SmtpClient client = CreateSmtpClient(config))
                {
                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(config.Options.From, "Star Citizen Gimp"),
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

        private void SendBadGimpEmail(BadGimpViewModel model, BadGimpReports bgr, ScGimpWebConfig config)
        {
            try
            {
                using (SmtpClient client = CreateSmtpClient(config))
                {
                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(config.Options.From, "Star Citizen Gimp"),
                        IsBodyHtml = false
                    };

                    mailMessage.To.Add(config.TraceRecipients);

                    mailMessage.Body = FormatBadGimpBody(model, bgr);
                    mailMessage.Subject = $"Star Citizen Gimp: There are currently {bgr.Count} reports of a bad gimp.";

                    client.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // TODO log
                Trace.WriteLine(ex);
            }
        }

        private string FormatBadGimpBody(BadGimpViewModel model, BadGimpReports bgr)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("Model message: ");
            builder.AppendLine(model.Message);
            builder.AppendLine("JSON:");
            builder.Append(JsonConvert.SerializeObject(bgr, Formatting.Indented));
            
            return builder.ToString();
        }

        private string FormatWelcomeBody()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("<div>");
            builder.AppendLine("<p>Greetings Citizen,</p>");
            builder.AppendLine("<p>You have been subscribed to the Star Citizen Gimp bot notification service.</p>");
            builder.AppendLine("<p>This is a free sevice, but it does cost something to host the services on the cloud (about $80 USD per month), as such any <a href=\"https://scgimp.com#donations\">donations</a> are greatly appreciated.</p>");
            builder.AppendLine("<p>If you wish to unsubscribe from these notifications at a later date you can <a href=\"https://scgimp.com/home/unsubscribe\">unsubscribe</a> by clicking <a href=\"https://scgimp.com/home/unsubscribe\">here</a>.</p>");
            builder.AppendLine("<p>Happy gaming and see you in the verse!</p>");
            builder.AppendLine("</div>");

            return builder.ToString();
        }

        private async Task<bool> VerifyGoogleRecaptcha(string recaptchaResponse)
        {
            try
            {
                string remoteIp = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                string url = "https://www.google.com/recaptcha/api/siteverify";
                ScGimpWebConfig config = new ScGimpWebConfig();
                HttpClient client = new HttpClient();
                FormUrlEncodedContent content = new FormUrlEncodedContent(new KeyValuePair<string, string>[] {
                    new KeyValuePair<string, string>("secret", config.RecaptchaKey),
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

        private string GetRemoteIpAddress()
        {
            return _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }
    }
}
