using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StarCitizen.Gimp.Core
{
    // Delegates
    public delegate void RssFeedUpdateEventHandler(object sender, RssFeedUpdateEventArgs e);
    public delegate void RssFeedPollEventHandler(object sender, RssFeedPollEventArgs e);
    public delegate void StoreFeedUpdateEventHandler(object sender, StoreFeedUpdateEventArgs e);
    public delegate void StoreFeedPollEventHandler(object sender, StoreFeedPollEventArgs e);
    public delegate void ErrorEventHandler(object sender, Exception e);

    /// <summary>
    /// Provides functionality to parse the RSI RSS feed, store feed and send notifications to subscribers when updates are detected.
    /// </summary>
    public class ScGimp : IDisposable
    {
        /// <summary>
        /// The feed update event handler.
        /// </summary>
        public RssFeedUpdateEventHandler RssFeedUpdate { get; set; }
        

        /// <summary>
        /// The feed polled event handler.
        /// </summary>
        public RssFeedPollEventHandler RssFeedPoll { get; set; }

        /// <summary>
        /// The feed update event handler.
        /// </summary>
        public StoreFeedUpdateEventHandler StoreFeedUpdate { get; set; }


        /// <summary>
        /// The feed polled event handler.
        /// </summary>
        public StoreFeedPollEventHandler StoreFeedPoll { get; set; }

        /// <summary>
        /// The error event handler.
        /// </summary>
        public ErrorEventHandler Error { get; set; }
        
        /// <summary>
        /// The exceptions if any encountered.
        /// </summary>
        public List<Exception> Errors { get; private set; }

        /// <summary>
        /// A list of the RSS feed items retrieved in the last process call.
        /// </summary>
        public List<FeedItem> RssFeed { get; private set; }

        /// <summary>
        /// A list of the store feed items retrieved in the last process call.
        /// </summary>
        public List<StoreItem> StoreFeed { get; private set; }

        /// <summary>
        /// A flag indicating if SC Gimp is actively monitoring the Star Citizen feeds and APIs.
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// The configuration options.
        /// </summary>
        public ScGimpOptions Options
        {
            get
            {
                return _options;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static readonly string __homeUrl = "https://robertsspaceindustries.com";

        /// <summary>
        /// 
        /// </summary>
        private static readonly string __getSkusUrl = "https://robertsspaceindustries.com/api/store/getSkus";

        /// <summary>
        /// 
        /// </summary>
        private static readonly string __rssUrl = "https://robertsspaceindustries.com/comm-link/rss";

        #region Compiled regular expressions
        /// <summary>
        /// 
        /// </summary>
        private static readonly RegexOptions __defaultRegexOptions =
            RegexOptions.Compiled |
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant;

        /// <summary>
        /// 
        /// </summary>
        private static readonly TimeSpan __defaultRegexTimeout = TimeSpan.FromSeconds(1d);

        /// <summary>
        /// 
        /// </summary>
        private static readonly Regex __discordWebhookRegex = new Regex
        (
           @"^https://discordapp.com/api/webhooks/[0-9]*/\S*$",
           __defaultRegexOptions,
           __defaultRegexTimeout
        );

        /// <summary>
        /// 
        /// </summary>
        private static readonly Regex __salesRegex = new Regex
        (
            @"(sale)|(lti)|(package)|(warbond)|(vip)|(limited)|(promotion)",
            __defaultRegexOptions,
            __defaultRegexTimeout
        );

        /// <summary>
        /// 
        /// </summary>
        private static readonly Regex __thisWeekRegex = new Regex
        (
            @"(this week in star citizen)",
            __defaultRegexOptions,
            __defaultRegexTimeout
        );

        /// <summary>
        /// 
        /// </summary>
        private static readonly Regex __eventRegex = new Regex
        (
            @"(event)",
            __defaultRegexOptions,
            __defaultRegexTimeout
        );

        /// <summary>
        /// 
        /// </summary>
        private static readonly Regex __portfolioRegex = new Regex
        (
            @"(portfolio)",
            __defaultRegexOptions,
            __defaultRegexTimeout
        );

        /// <summary>
        /// 
        /// </summary>
        private static readonly Regex __callingAllDevsRegex = new Regex
        (
            @"(calling all devs)",
            __defaultRegexOptions,
            __defaultRegexTimeout
        );

        /// <summary>
        /// 
        /// </summary>
        private static readonly Regex __reverseTheVerseRegex = new Regex
        (
            @"(reverse the verse)|(rtv)",
            __defaultRegexOptions,
            __defaultRegexTimeout
        );

        /// <summary>
        /// 
        /// </summary>
        private static readonly Regex __aroundTheVerseRegex = new Regex
        (
            @"(around the verse)|(atv)",
            __defaultRegexOptions,
            __defaultRegexTimeout
        );

        /// <summary>
        /// 
        /// </summary>
        private static readonly Regex __questionAndAnswerRegex = new Regex
        (
            @"(q\&a)",
            __defaultRegexOptions,
            __defaultRegexTimeout
        );

        /// <summary>
        /// 
        /// </summary>
        private static readonly Regex __galacticGuideRegex = new Regex
        (
            @"(galactic guide)",
            __defaultRegexOptions,
            __defaultRegexTimeout
        );

        /// <summary>
        /// 
        /// </summary>
        private static readonly Regex __subscriberRegex = new Regex
        (
            @"(subscriber)",
            __defaultRegexOptions,
            __defaultRegexTimeout
        );

        /// <summary>
        /// 
        /// </summary>
        private static readonly Regex __loremakersGuidToTheGalaxyRegex = new Regex
        (
            @"(loremaker)|(guide to the galaxy)",
            __defaultRegexOptions,
            __defaultRegexTimeout
        );

        /// <summary>
        /// 
        /// </summary>
        private static readonly Regex __monthlyStudioReportRegex = new Regex
        (
            @"(monthly studio report)",
            __defaultRegexOptions,
            __defaultRegexTimeout
        );

        /// <summary>
        /// 
        /// </summary>
        private static readonly Regex __bugsmashersRegex = new Regex
        (
            @"(bugsmashers)",
            __defaultRegexOptions,
            __defaultRegexTimeout
        );
        #endregion

        /// <summary>
        /// 
        /// </summary>
        private Task _worker;

        /// <summary>
        /// 
        /// </summary>
        private bool _killWorker;

        /// <summary>
        /// 
        /// </summary>
        private long _totalContentLength;

        /// <summary>
        /// 
        /// </summary>
        private ScGimpOptions _options;

        /// <summary>
        /// 
        /// </summary>
        private CookieContainer _cookies;

        /// <summary>
        /// 
        /// </summary>
        private HttpClientHandler _handler;

        /// <summary>
        /// http://byterot.blogspot.co.za/2016/07/singleton-httpclient-dns.html
        /// https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
        /// https://github.com/dotnet/corefx/issues/11224
        /// </summary>
        private HttpClient _client;

        /// <summary>
        /// 
        /// </summary>
        private ISubscriberProvider _subscriberProvider;

        /// <summary>
        /// 
        /// </summary>
        private IDiscordWebhookProvider _discordWebhookProvider;

        /// <summary>
        /// 
        /// </summary>
        private INotificationLogProvider _notificationLogProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriberProvider"></param>
        /// <param name="discordWebhookProvider"></param>
        /// <param name="notificationLogProvider"></param>
        public ScGimp(ISubscriberProvider subscriberProvider, IDiscordWebhookProvider discordWebhookProvider, INotificationLogProvider notificationLogProvider)
        {
            _subscriberProvider = subscriberProvider ?? throw new ArgumentNullException("subscriberProvider");
            _discordWebhookProvider = discordWebhookProvider ?? throw new ArgumentNullException("discordWebhookProvider");
            _notificationLogProvider = notificationLogProvider ?? throw new ArgumentNullException("notificationLogProvider");

            Initialize();
        }

        /// <summary>
        /// 
        /// </summary>
        private void Initialize()
        {
            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            IConfigurationRoot configuration = configBuilder.Build();

            _options = new ScGimpOptions()
            {
                DeliveryMethod = configuration["DeliveryMethod"] == "Network" ?
                    SmtpDeliveryMethod.Network :
                    (
                            configuration["DeliveryMethod"] == "PickupDirectoryFromIis" ?
                                SmtpDeliveryMethod.PickupDirectoryFromIis :
                                (
                                    configuration["DeliveryMethod"] == "SpecifiedPickupDirectory" ?
                                    SmtpDeliveryMethod.SpecifiedPickupDirectory : 
                                    (SmtpDeliveryMethod?)null
                                )
                    ),
                Domain = string.IsNullOrWhiteSpace(configuration["Domain"]) ? null : configuration["Domain"],
                EnableSsl = string.IsNullOrWhiteSpace(configuration["EnableSsl"]) ? (bool?)null : bool.Parse(configuration["EnableSsl"]),
                From = string.IsNullOrWhiteSpace(configuration["From"]) ? null : configuration["From"],
                Host = string.IsNullOrWhiteSpace(configuration["Host"]) ? null : configuration["Host"],
                Password = string.IsNullOrWhiteSpace(configuration["Password"]) ? null : configuration["Password"],
                Port = string.IsNullOrWhiteSpace(configuration["Port"]) ? (int?)null : int.Parse(configuration["Port"]),
                Sleep = string.IsNullOrWhiteSpace(configuration["Sleep"]) ? TimeSpan.FromSeconds(30d) : TimeSpan.FromSeconds(double.Parse(configuration["Sleep"])),
                TargetName = string.IsNullOrWhiteSpace(configuration["TargetName"]) ? null : configuration["TargetName"],
                UseDefaultCredentials = string.IsNullOrWhiteSpace(configuration["UseDefaultCredentials"]) ? (bool?)null : bool.Parse(configuration["UseDefaultCredentials"]),
                Username = string.IsNullOrWhiteSpace(configuration["Username"]) ? null : configuration["Username"]
            };

            _killWorker = false;
            _worker = null;
            _totalContentLength = 0L;
            _cookies = new CookieContainer();
            _handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                UseCookies = true,
                CookieContainer = _cookies
            };
            _client = new HttpClient(_handler, false)
            {
                Timeout = TimeSpan.FromSeconds(60)
            };
            Enabled = false;
            RssFeed = new List<FeedItem>();
            StoreFeed = new List<StoreItem>();
            Errors = new List<Exception>();
            Error += new ErrorEventHandler(OnError);

            SetServicePointConnectionLeaseTimeouts();
        }

        /// <summary>
        /// http://byterot.blogspot.co.za/2016/07/singleton-httpclient-dns.html
        /// https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
        /// https://github.com/dotnet/corefx/issues/11224
        /// </summary>
        private void SetServicePointConnectionLeaseTimeouts()
        {
            ServicePoint homeServicePoint = ServicePointManager.FindServicePoint(new Uri(__homeUrl));
            ServicePoint getSkusServicePoint = ServicePointManager.FindServicePoint(new Uri(__getSkusUrl));
            ServicePoint rssServicePoint = ServicePointManager.FindServicePoint(new Uri(__rssUrl));
            int timeout = 60 * 1000; // 1 minute

            homeServicePoint.ConnectionLeaseTimeout = timeout;
            getSkusServicePoint.ConnectionLeaseTimeout = timeout;
            rssServicePoint.ConnectionLeaseTimeout = timeout;
        }

        /// <summary>
        /// Enables monitoring.
        /// </summary>
        public void Start()
        {
            if (Enabled)
            {
                return;
            }

            if (_worker == null)
            {
                _worker = Task.Run(Worker);
            }

            Enabled = true;
        }

        /// <summary>
        /// Disables monitoring.
        /// </summary>
        public void Stop()
        {
            Enabled = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task Worker()
        {
            while (!_killWorker)
            {
                try
                {
                    if (Enabled)
                    {
                        await Process();
                    }

                    System.Threading.Thread.Sleep(_options.Sleep);
                }
                catch (Exception ex)
                {
                    Error?.Invoke(this, ex);
                }
            }
        }

        /// <summary>
        /// Downloads the official RSS communication feed from the RSI website and parses it.
        /// If any changes are detected feed update event handler is invoked and 
        /// if none exist the feed poll event handler is invoked.
        /// </summary>
        /// <returns>An array of the items parsed from the RSS feed.</returns>
        public async Task Process()
        {
            Task[] tasks = new Task[]
            {
                ProcessRssFeed(),
                ProcessStoreFeed()
            };

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task ProcessStoreFeed()
        {
            try
            {
                using (HttpResponseMessage getHomeResponse = await _client.GetAsync(__homeUrl))
                {
                    _totalContentLength += getHomeResponse.Content.Headers.ContentLength ?? 0L;
                    long contentLength = getHomeResponse.Content.Headers.ContentLength ?? 0L;

                    if (!getHomeResponse.IsSuccessStatusCode)
                    {
                        if (getHomeResponse.StatusCode == HttpStatusCode.BadGateway)
                        {
                            throw new RsiBadGatewayException($"HTTP get request failed for URL: {__homeUrl} with status {getHomeResponse.StatusCode.ToString()}");
                        }
                        else
                        {
                            throw new Exception($"HTTP get request failed for URL: {__homeUrl} with status {getHomeResponse.StatusCode.ToString()}");
                        }
                    }

                    string key = "x-rsi-token";

                    if (!_client.DefaultRequestHeaders.Any(h => h.Key == key))
                    {
                        // Prep headers so we can start querying the API.
                        Cookie rsiToken = _cookies.GetCookies(new Uri(__homeUrl)).Cast<Cookie>().FirstOrDefault(c => string.Compare(c.Name, "Rsi-Token", true) == 0);

                        _client.DefaultRequestHeaders.Add(key, rsiToken.Value);
                    }

                    StoreCrawlResult crawlResult = await CrawlTheStore(_client);

                    IEqualityComparer<StoreItem> comparer = new StoreItemEqualityComparer();
                    StoreItem[] storeFeedUpdates = crawlResult.StoreFeed.Except(StoreFeed, comparer).ToArray();

                    if (StoreFeed.Count() == 0 &&
                        storeFeedUpdates.Length > 0)
                    {
                        // We just started so do not notify, just initialize for now.
                        StoreFeed.AddRange(crawlResult.StoreFeed);

                        StoreFeedPoll?.Invoke(this, new StoreFeedPollEventArgs(_totalContentLength, crawlResult.ContentLength + contentLength));
                    }
                    else if (StoreFeed.Count() > 0 &&
                        storeFeedUpdates != null &&
                        storeFeedUpdates.Length > 0)
                    {
                        // We have items from a previous process and we have changes.
                        StoreFeed.Clear();
                        StoreFeed.AddRange(crawlResult.StoreFeed);
                        await Notify(storeFeedUpdates);
                        StoreFeedUpdate?.Invoke(this, new StoreFeedUpdateEventArgs(storeFeedUpdates, _totalContentLength, crawlResult.ContentLength + contentLength));
                    }
                    else
                    {
                        StoreFeedPoll?.Invoke(this, new StoreFeedPollEventArgs(_totalContentLength, crawlResult.ContentLength + contentLength));
                    }
                }
            }
            catch (RsiBadGatewayException) { }
            catch (Exception ex)
            {
                Error?.Invoke(this, new Exception("Failed to process the store API feed. Please see inner exception for more information.", ex));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeFeedDoc"></param>
        /// <returns></returns>
        private IEnumerable<StoreItem> ParseStoreItems(XDocument storeFeedDoc)
        {
            if (storeFeedDoc == null ||
                storeFeedDoc.Root == null ||
                storeFeedDoc.Root.Elements() == null ||
                storeFeedDoc.Root.Elements().Count() == 0)
            {
                return new StoreItem[0];
            }

            return storeFeedDoc.Root.Elements().Select(i => new StoreItem(i));
        }

        public class StoreCrawlResult
        {
            public List<StoreItem> StoreFeed { get; set; }
            public long ContentLength { get; set; }
        }

        public class StorePageCrawlResult
        {
            public IEnumerable<StoreItem> StoreFeed { get; set; }
            public long ContentLength { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task<StoreCrawlResult> CrawlTheStore(HttpClient client)
        {
            StoreCrawlResult result = new StoreCrawlResult()
            {
                StoreFeed = new List<StoreItem>(),
                ContentLength = 0L
            };
            int page = 1;

            while (page >= 1 && Enabled && !_killWorker)
            {
                StorePageCrawlResult storePage = await GetStorePage(client, page);

                if (storePage.StoreFeed.Count() > 0)
                {
                    result.StoreFeed.AddRange(storePage.StoreFeed);
                    result.ContentLength += storePage.ContentLength;

                    page++;
                }
                else
                {
                    page = -1;
                }
            }

            page = 1;

            while (page >= 1 && Enabled && !_killWorker)
            {
                StorePageCrawlResult storePage = await GetStorePage(client, page, "", "store", "", "skus", "pledge", "game-packages");

                if (storePage.StoreFeed.Count() > 0)
                {
                    result.StoreFeed.AddRange(storePage.StoreFeed);
                    result.ContentLength += storePage.ContentLength;

                    page++;
                }
                else
                {
                    page = -1;
                }
            }

            page = 1;

            while (page >= 1 && Enabled && !_killWorker)
            {
                StorePageCrawlResult storePage = await GetStorePage(client, page, "72", "store", "", "skus", "pledge", "extras");

                if (storePage.StoreFeed.Count() > 0)
                {
                    result.StoreFeed.AddRange(storePage.StoreFeed);
                    result.ContentLength += storePage.ContentLength;

                    page++;
                }
                else
                {
                    page = -1;
                }
            }

            if (result.StoreFeed.Count() > 1)
            {
                StoreItemEqualityComparer equalityComparer = new StoreItemEqualityComparer();

                result.StoreFeed = result.StoreFeed.Distinct(equalityComparer).ToList();
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="page"></param>
        /// <param name="productId"></param>
        /// <param name="sort"></param>
        /// <param name="search"></param>
        /// <param name="itemType"></param>
        /// <param name="storeFront"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private async Task<StorePageCrawlResult> GetStorePage(HttpClient client, int page, string productId = "", string sort = "store", string search = "", string itemType = "skus", string storeFront = "pledge", string type = "extras")
        {
            StorePageCrawlResult result = new StorePageCrawlResult()
            {
                ContentLength = 0L,
                StoreFeed = null
            };
            string getSkusJsonPayload = $"{{ \"product_id\": \"{productId}\", \"sort\": \"{sort}\", \"search\": \"{search}\", \"itemType\": \"{itemType}\", \"storefront\": \"{storeFront}\", \"type\": \"{type}\",\"page\":{page} }}";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(getSkusJsonPayload);
            ByteArrayContent byteContent = new ByteArrayContent(buffer);

            using (HttpResponseMessage getSkusResponse = await client.PostAsync(__getSkusUrl, byteContent))
            {
                _totalContentLength += getSkusResponse.Content.Headers.ContentLength ?? 0L;
                result.ContentLength = getSkusResponse.Content.Headers.ContentLength ?? 0L;

                if (!getSkusResponse.IsSuccessStatusCode)
                {
                    if (getSkusResponse.StatusCode == HttpStatusCode.BadGateway)
                    {
                        throw new RsiBadGatewayException($"HTTP post request failed for URL: {__getSkusUrl} with status {getSkusResponse.StatusCode.ToString()}");
                    }
                    else
                    {
                        throw new Exception($"HTTP post request failed for URL: {__getSkusUrl} with status {getSkusResponse.StatusCode.ToString()}");
                    }
                }

                // Read content.
                string postResponseContent = await getSkusResponse.Content.ReadAsStringAsync();

                dynamic jsonContent = Newtonsoft.Json.JsonConvert.DeserializeObject(postResponseContent);

                if (jsonContent?.success == 1)
                {
                    if (jsonContent.data.rowcount != 0 &&
                        jsonContent.data.html.ToString().Length > 0)
                    {
                        XDocument doc = XDocument.Parse(jsonContent.data.html.ToString().Replace("&", "and"));

                        result.StoreFeed = ParseStoreItems(doc);
                    }
                    else
                    {
                        result.StoreFeed = new StoreItem[0];
                    }
                }
                else
                {
                    throw new Exception($"Failed to get data from store API.\r\n" + postResponseContent);
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task ProcessRssFeed()
        {
            try
            {
                // Download the RSS feed.
                bool bustCache = false;
                string url = bustCache ? $"{__rssUrl}?cachebuster={Guid.NewGuid().ToString("N")}" : __rssUrl;

                using (HttpResponseMessage response = await _client.GetAsync(url))
                {
                    _totalContentLength += response.Content.Headers.ContentLength ?? 0L;

                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.BadGateway)
                        {
                            throw new RsiBadGatewayException($"HTTP get request failed for URL: {url} with status {response.StatusCode.ToString()}");
                        }
                        else
                        {
                            throw new Exception($"HTTP get request failed for URL: {url} with status {response.StatusCode.ToString()}");
                        }
                    }

                    // Read content.
                    string rss = await response.Content.ReadAsStringAsync();

                    // Parse the content.
                    XDocument doc = XDocument.Parse(rss);

                    IEnumerable<FeedItem> rssFeed = ParseFeedItems(doc);

                    rssFeed = SetFeedItemTypes(rssFeed);

                    IEqualityComparer<FeedItem> comparer = new FeedItemEqualityComparer();
                    FeedItem[] rssFeedUpdates = rssFeed.Except(RssFeed, comparer).ToArray();

                    if (RssFeed.Count() == 0 &&
                        rssFeedUpdates.Length > 0)
                    {
                        // We just started so do not notify, just initialize for now.
                        RssFeed.AddRange(rssFeed);

                        RssFeedPoll?.Invoke(this, new RssFeedPollEventArgs(_totalContentLength, response.Content.Headers.ContentLength ?? 0L));
                    }
                    else if (
                        RssFeed.Count() > 0 &&
                        rssFeedUpdates != null &&
                        rssFeedUpdates.Length > 0)
                    {
                        // We have items from a previous process and we have changes.
                        RssFeed.Clear();
                        RssFeed.AddRange(rssFeed);
                        await Notify(rssFeedUpdates);
                        RssFeedUpdate?.Invoke(this, new RssFeedUpdateEventArgs(rssFeedUpdates, _totalContentLength, response.Content.Headers.ContentLength ?? 0L));
                    }
                    else
                    {
                        RssFeedPoll?.Invoke(this, new RssFeedPollEventArgs(_totalContentLength, response.Content.Headers.ContentLength ?? 0L));
                    }
                }
            }
            catch (RsiBadGatewayException) { }
            catch (Exception ex)
            {
                Error?.Invoke(this, new Exception("Failed to process the comm link RSS feed. Please see inner exception for more information.", ex));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updates"></param>
        /// <returns></returns>
        public async Task Notify(IEnumerable<object> updates)
        {
            try
            {
                using (SmtpClient client = CreateSmtpClient())
                {
                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(_options.From, "Star Citizen Gimp"),
                        IsBodyHtml = true
                    };

                    IEnumerable<ScGimpSubscriber> subs = await _subscriberProvider.GetSubscribersAsync();

                    if (!subs.Any())
                    {
                        return;
                    }

                    Regex emailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
                    SubscriberEqualityComparer comparer = new SubscriberEqualityComparer();

                    subs = subs.Where(s => emailRegex.IsMatch(s.Email)).Distinct(comparer);

                    var doubleVerifiedSubs = new List<string>();

                    foreach (var sub in subs)
                    {
                        try
                        {
                            MailAddress address = new MailAddress(sub.Email);

                            doubleVerifiedSubs.Add(sub.Email);
                        }
                        catch
                        {

                        }
                    }

                    string recipients = string.Join(",", doubleVerifiedSubs);

                    mailMessage.Bcc.Add(recipients);

                    string body = FormatEmailNotificationBody(updates);

                    mailMessage.Body = body;
                    mailMessage.Subject = $"{updates.Count().ToString()} content update(s) detected on RSI.";

                    client.Send(mailMessage);

                    try
                    {
                        ScGimpNotification logEntry = new ScGimpNotification()
                        {
                            Body = body,
                            Items = JsonConvert.SerializeObject(updates),
                            NotificationType = updates is StoreItem[] ? "Store" : "Comm link",
                            Recipients = recipients,
                            Medium = "Email"
                        };

                        await _notificationLogProvider.WriteAsync(logEntry);
                    }
                    catch (Exception logEx)
                    {
                        Error?.Invoke(this, new Exception("Failed to log notification. Please see inner exception for more information.", logEx));
                    }
                }
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, new Exception("Failed to notify subscribers via email. Please see inner exception for more information.", ex));
            }

            // Notify discord webhooks
            try
            {
                IEnumerable<ScGimpDiscordWebhook> discordWebhooks = await _discordWebhookProvider.GetDiscordWebhooksAsync();

                if (!discordWebhooks.Any())
                {
                    return;
                }

                string message = FormatDiscordNotificationBody(updates);

                Parallel.ForEach(discordWebhooks, async wh => 
                {
                    await NotifyDiscordWebhook(wh.Url, message);
                });

                try
                {
                    ScGimpNotification logEntry = new ScGimpNotification()
                    {
                        Body = message,
                        Items = JsonConvert.SerializeObject(updates),
                        NotificationType = updates is StoreItem[] ? "Store" : "Comm-link",
                        Recipients = string.Join(",", discordWebhooks.Select(wh => wh.Url)),
                        Medium = "Discord Webhook"
                    };

                    await _notificationLogProvider.WriteAsync(logEntry);
                }
                catch (Exception logEx)
                {
                    Error?.Invoke(this, new Exception("Failed to log notification. Please see inner exception for more information.", logEx));
                }
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, new Exception("Failed to notify Discord Webhooks. Please see inner exception for more information.", ex));
            }
        }

        private async Task NotifyDiscordWebhook(string url, string message)
        {
            try
            {
                if (__discordWebhookRegex.IsMatch(url))
                {
                    string postUrl = $"{url}?wait=true";
                    ServicePoint servicePoint = ServicePointManager.FindServicePoint(new Uri(postUrl));
                    int timeout = 60 * 1000; // 1 minute

                    servicePoint.ConnectionLeaseTimeout = timeout;

                    string jsonPayload = $"{{ \"content\": \"{message}\" }}";

                    using (HttpResponseMessage response = await _client.PostAsync(postUrl, new StringContent(jsonPayload, Encoding.UTF8, "application/json")))
                    {
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == HttpStatusCode.BadGateway)
                            {
                                Error?.Invoke(this, new RsiBadGatewayException($"Failed to notify Discord Webhook ({url}). The Discord server responded with {response.StatusCode.ToString()}."));
                            }
                            else
                            {
                                Error?.Invoke(this, new Exception($"Failed to notify Discord Webhook ({url}). The Discord server responded with {response.StatusCode.ToString()}."));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, new Exception("Failed to notify Discord Webhooks. Please see inner exception for more information.", ex));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private SmtpClient CreateSmtpClient()
        {
            SmtpClient client = new SmtpClient(_options.Host);

            if (_options.Port.HasValue)
            {
                client.Port = _options.Port.Value;
            }

            if (_options.UseDefaultCredentials.HasValue)
            {
                client.UseDefaultCredentials = _options.UseDefaultCredentials.Value;
            }

            if (_options.EnableSsl.HasValue)
            {
                client.EnableSsl = _options.EnableSsl.Value;
            }

            if (_options.DeliveryMethod.HasValue)
            {
                client.DeliveryMethod = _options.DeliveryMethod.Value;
            }

            if (!string.IsNullOrWhiteSpace(_options.TargetName))
            {
                client.TargetName = _options.TargetName;
            }

            if (!string.IsNullOrWhiteSpace(_options.Username) &&
                !string.IsNullOrWhiteSpace(_options.Password) &&
                !string.IsNullOrWhiteSpace(_options.Domain))
            {
                client.Credentials = new NetworkCredential(_options.Username, _options.Password, _options.Domain); 
            } else if (!string.IsNullOrWhiteSpace(_options.Username) &&
                !string.IsNullOrWhiteSpace(_options.Password))
            {
                client.Credentials = new NetworkCredential(_options.Username, _options.Password);
            }

            return client;
        }

        private string FormatDiscordNotificationBody(IEnumerable<object> items)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("Greetings Citizen,");
            builder.AppendLine();
            builder.AppendLine("The following content update(s) were detected on RSI:");
            builder.AppendLine();

            int i = 0;

            foreach (var item in items)
            {
                if (item is FeedItem feedItem)
                {
                    builder.AppendLine
                    (
                        $"{(i + 1).ToString()}. {feedItem.Title} {feedItem.Link} was published at {feedItem.PublishDate.ToString()}"
                    );
                }
                else if (item is StoreItem storeItem)
                {
                    builder.AppendLine
                    (
                        $"{(i + 1).ToString()}. {storeItem.Title} {storeItem.Currency} {storeItem.Cost.ToString()} {storeItem.Url}"
                    );
                }

                i++;
            }

            builder.AppendLine();
            builder.AppendLine("You can go to https://scgimp.com/home/deletediscordwebhook to deregister your Discord Webhook at any time.");

            return builder.ToString().Replace("\r", string.Empty).Replace("\n", "\\n");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private string FormatEmailNotificationBody(IEnumerable<object> items)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("<div>");
            builder.AppendLine("<p>Greetings Citizen,</p>");
            builder.AppendLine("<p>The following content update(s) were detected on RSI:</p>");

            int i = 0;

            foreach (var item in items)
            {
                if (item is FeedItem feedItem)
                {
                    builder.AppendLine
                    (
                        $"<div>{(i + 1).ToString()} <a href=\"{feedItem.Link}\" alt=\"{feedItem.Title}\">{feedItem.Title}</a> was published at {feedItem.PublishDate.ToString()}</div>"
                    );
                }
                else if (item is StoreItem storeItem)
                {
                    builder.AppendLine
                    (
                        $"<div>{(i + 1).ToString()}. <a href=\"{storeItem.Url}\" alt=\"{storeItem.Title}\">{storeItem.Title}</a> {storeItem.Currency} {storeItem.Cost.ToString()}</div>"
                    );
                }

                i++;
            }

            builder.AppendLine("<p>If you do not wish to receive these notifications anymore you can <a href=\"https://scgimp.com/home/unsubscribe\">unsubscribe</a> by clicking <a href=\"https://scgimp.com/home/unsubscribe\">here</a>.</p>");

            builder.AppendLine("</div>");

            return builder.ToString();
        }

        /// <summary>
        /// Converts the Star Citizen RSS feed xml document to a feed item array.
        /// </summary>
        /// <param name="rssFeedDoc">The RSS feed.</param>
        /// <returns>An enumerable of feed items.</returns>
        private IEnumerable<FeedItem> ParseFeedItems(XDocument rssFeedDoc)
        {
            if (rssFeedDoc == null ||
                rssFeedDoc.Root == null ||
                rssFeedDoc.Root.Descendants() == null ||
                rssFeedDoc.Root.Descendants().Count() == 0)
            {
                return new FeedItem[0];
            }

            IEnumerable<XElement> elements = rssFeedDoc
                .Root
                .Descendants()
                .First(i => i.Name.LocalName == "channel")
                .Elements()
                .Where(i => i.Name.LocalName == "item");
            IEnumerable<FeedItem> items = elements
                .Select(e => new FeedItem(e))
                .OrderByDescending(i => i.PublishDate);

            return items;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rssFeed"></param>
        /// <returns></returns>
        private IEnumerable<FeedItem> SetFeedItemTypes(IEnumerable<FeedItem> rssFeed)
        {
            foreach (FeedItem item in rssFeed)
            {
                if (__salesRegex.IsMatch(item.Title))
                {
                    item.Type = FeedItemType.Sale;
                }
                else if (__aroundTheVerseRegex.IsMatch(item.Title))
                {
                    item.Type = FeedItemType.AroundTheVerse;
                }
                else if (__bugsmashersRegex.IsMatch(item.Title))
                {
                    item.Type = FeedItemType.Bugsmashers;
                }
                else if (__callingAllDevsRegex.IsMatch(item.Title))
                {
                    item.Type = FeedItemType.CallingAllDevs;
                }
                else if (__eventRegex.IsMatch(item.Title))
                {
                    item.Type = FeedItemType.Event;
                }
                else if (__galacticGuideRegex.IsMatch(item.Title))
                {
                    item.Type = FeedItemType.GalacticGuide;
                }
                else if (__loremakersGuidToTheGalaxyRegex.IsMatch(item.Title))
                {
                    item.Type = FeedItemType.LoremakersGuideToTheGalaxy;
                }
                else if (__monthlyStudioReportRegex.IsMatch(item.Title))
                {
                    item.Type = FeedItemType.MonthlyStudioReport;
                }
                else if (__portfolioRegex.IsMatch(item.Title))
                {
                    item.Type = FeedItemType.Portfolio;
                }
                else if (__questionAndAnswerRegex.IsMatch(item.Title))
                {
                    item.Type = FeedItemType.QuestionsAndAnswers;
                }
                else if (__reverseTheVerseRegex.IsMatch(item.Title))
                {
                    item.Type = FeedItemType.ReverseTheVerse;
                }
                else if (__subscriberRegex.IsMatch(item.Title))
                {
                    item.Type = FeedItemType.Subscriber;
                }
                else if (__thisWeekRegex.IsMatch(item.Title))
                {
                    item.Type = FeedItemType.ThisWeekInStarCitizen;
                }
                else
                {
                    item.Type = FeedItemType.Unknown;
                }
            }

            return rssFeed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnError(object sender, Exception e)
        {
            Errors.Add(e);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    try
                    {
                        if (_worker != null)
                        {
                            _killWorker = true;

                            _worker.Wait();

                            _worker.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Write(ex);
                    }

                    try
                    {
                        _client.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Write(ex);
                    }

                    try
                    {
                        _handler.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Write(ex);
                    }
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.
                RssFeed = null;
                StoreFeed = null;
                _worker = null;
                _client = null;
                _handler = null;
                _cookies = null;
                _subscriberProvider = null;
                disposedValue = true;
            }
        }

        // Override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ScGimp() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // Uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
