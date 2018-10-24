using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StarCitizen.Gimp.Core
{
    // Enums
    /// <summary>
    /// The ScGimp bot process type.
    /// </summary>
    public enum ScGimpProcessType
    {
        /// <summary>
        /// Spectrum feed proccess type monitoring.
        /// </summary>
        SpectrumFeed,
        /// <summary>
        /// Store feed proccess type monitoring.
        /// </summary>
        StoreFeed,
        /// <summary>
        /// RSS feed proccess type monitoring.
        /// </summary>
        RssFeed,
        /// <summary>
        /// All proccesses type monitoring.
        /// </summary>
        All
    }

    // Delegates
    public delegate void ProcessingEventHandler(object sender, ProcessingEventArgs e);
    public delegate void ProcessedEventHandler(object sender, ProcessedEventArgs e);
    public delegate void RssFeedUpdateEventHandler(object sender, RssFeedUpdateEventArgs e);
    public delegate void RssFeedPollEventHandler(object sender, RssFeedPollEventArgs e);
    public delegate void StoreFeedUpdateEventHandler(object sender, StoreFeedUpdateEventArgs e);
    public delegate void StoreFeedPollEventHandler(object sender, StoreFeedPollEventArgs e);
    public delegate void SpectrumFeedUpdateEventHandler(object sender, SpectrumFeedUpdateEventArgs e);
    public delegate void SpectrumFeedPollEventHandler(object sender, SpectrumFeedPollEventArgs e);
    public delegate void ErrorEventHandler(object sender, Exception e);

    /// <summary>
    /// Provides functionality to parse the RSI RSS feed, store feed and send notifications to subscribers when updates are detected.
    /// </summary>
    public class ScGimp : IDisposable
    {
        /// <summary>
        /// The processing event handler.
        /// </summary>
        public ProcessingEventHandler Processing { get; set; }

        /// <summary>
        /// The processed event handler.
        /// </summary>
        public ProcessedEventHandler Processed { get; set; }

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
        /// The feed update event handler.
        /// </summary>
        public SpectrumFeedUpdateEventHandler SpectrumFeedUpdate { get; set; }


        /// <summary>
        /// The feed polled event handler.
        /// </summary>
        public SpectrumFeedPollEventHandler SpectrumFeedPoll { get; set; }

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
        /// A list of all the retrieved spectrum threads in the last process call.
        /// </summary>
        public List<SpectrumThread> SpectrumFeed { get; private set; }

        /// <summary>
        /// The date and time when the last worker update occurred.
        /// </summary>
        public DateTimeOffset? LastUpdated { get; private set; }

        /// <summary>
        /// The worker task status.
        /// </summary>
        public TaskStatus? Status
        {
            get
            {
                return _worker?.Status;
            }
        }

        /// <summary>
        /// The configuration options.
        /// </summary>
        public ScGimpOptions Options { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public ConcurrentQueue<ScGimpNotification> NotificationHistory { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        private static readonly TimeSpan __defaultHttpClientTimeout = TimeSpan.FromSeconds(60);

        /// <summary>
        /// 
        /// </summary>
        private static readonly int __defaultConnectionLeaseTimeout = 60 * 1000; // 1 minute

        /// <summary>
        /// 
        /// </summary>
        private static readonly string __getThreadsUrl = "https://robertsspaceindustries.com/api/spectrum/forum/channel/threads";

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

        private CancellationTokenSource _cancellationTokenSource;

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
        private SemaphoreSlim _authorizeSemaphoreSlim;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriberProvider"></param>
        /// <param name="discordWebhookProvider"></param>
        /// <param name="notificationLogProvider"></param>
        /// <param name="options"></param>
        public ScGimp
        (
            ISubscriberProvider subscriberProvider, 
            IDiscordWebhookProvider discordWebhookProvider, 
            INotificationLogProvider notificationLogProvider, 
            ScGimpOptions options = null
        )
        {
            _subscriberProvider = subscriberProvider ?? throw new ArgumentNullException("subscriberProvider");
            _discordWebhookProvider = discordWebhookProvider ?? throw new ArgumentNullException("discordWebhookProvider");
            _notificationLogProvider = notificationLogProvider ?? throw new ArgumentNullException("notificationLogProvider");

            if (options == null)
            {
                options = new ScGimpOptions();
            }

            Options = options;

            Initialize();
        }

        /// <summary>
        /// Gets the process type from the configuration.
        /// </summary>
        /// <returns>The proces type.</returns>
        public ScGimpProcessType GetScGimpProcessType()
        {
            return Options.ProcessType ?? ScGimpProcessType.All;
        }

        /// <summary>
        /// 
        /// </summary>
        private void Initialize()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _authorizeSemaphoreSlim = new SemaphoreSlim(1, 1);
            _worker = null;
            _cookies = new CookieContainer();
            _handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                UseCookies = true,
                CookieContainer = _cookies
            };
            _client = new HttpClient(_handler, false)
            {
                Timeout = __defaultHttpClientTimeout
            };
            LastUpdated = null;
            NotificationHistory = new ConcurrentQueue<ScGimpNotification>();
            RssFeed = new List<FeedItem>();
            StoreFeed = new List<StoreItem>();
            SpectrumFeed = new List<SpectrumThread>();
            Errors = new List<Exception>();
            Error += OnError;

            /// http://byterot.blogspot.co.za/2016/07/singleton-httpclient-dns.html
            /// https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            /// https://github.com/dotnet/corefx/issues/11224
            ServicePoint homeServicePoint = ServicePointManager.FindServicePoint(new Uri(__homeUrl));
            ServicePoint getSkusServicePoint = ServicePointManager.FindServicePoint(new Uri(__getSkusUrl));
            ServicePoint rssServicePoint = ServicePointManager.FindServicePoint(new Uri(__rssUrl));

            homeServicePoint.ConnectionLeaseTimeout = __defaultConnectionLeaseTimeout;
            getSkusServicePoint.ConnectionLeaseTimeout = __defaultConnectionLeaseTimeout;
            rssServicePoint.ConnectionLeaseTimeout = __defaultConnectionLeaseTimeout;
        }

        /// <summary>
        /// Enables monitoring.
        /// </summary>
        public async Task Start()
        {
            LastUpdated = null;
            RssFeed.Clear();
            SpectrumFeed.Clear();
            StoreFeed.Clear();

            if (_worker != null)
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();

                    await _worker;
                }

                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }

            _worker = Task.Run(Worker, _cancellationTokenSource.Token);
        }

        /// <summary>
        /// Disables monitoring.
        /// </summary>
        public async Task Stop()
        {
            if (_worker != null &&
                _cancellationTokenSource != null &&
                !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();

                await _worker;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task Worker()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    await Process();

                    await Task.Delay(GetTimeSpanToSleep(), _cancellationTokenSource.Token);
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    Error?.Invoke(this, ex);
                }
            }
        }

        /// <summary>
        /// Gets the duration that the worker thread needs to sleep based on the current date and time and the modifiers in the options.
        /// </summary>
        /// <returns>The duration of the worker to sleep.</returns>
        public TimeSpan GetTimeSpanToSleep()
        {
            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            DateTimeOffset cigLocalDateTime = DateTimeOffset.UtcNow.ToOffset(zone.GetUtcOffset(DateTimeOffset.Now));
            TimeSpan cigLocalTimeOfDay = cigLocalDateTime.TimeOfDay;

            if (cigLocalDateTime.DayOfWeek == DayOfWeek.Sunday ||
                cigLocalDateTime.DayOfWeek == DayOfWeek.Saturday ||
                cigLocalTimeOfDay > Options.CigWorkingHoursEnd ||
                cigLocalTimeOfDay < Options.CigWorkingHoursStart)
            {
                TimeSpan throttledSleep = Options.Sleep.Multiply(Options.AfterHoursSleepMultiplier);

                return throttledSleep;
            }
            else if (cigLocalTimeOfDay > Options.CigCommLinkEnd ||
                     cigLocalTimeOfDay < Options.CigCommLinkStart)
            {
                TimeSpan throttledSleep = Options.Sleep.Multiply(Options.OutsideCommLinkSleepMultiplier);

                return throttledSleep;
            }
            else
            {
                return Options.Sleep;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Process()
        {
            Processing?.Invoke(this, new ProcessingEventArgs(GetScGimpProcessType()));

            List<Task> tasks = new List<Task>();
            ScGimpProcessType processType = GetScGimpProcessType();

            switch (processType)
            {
                case ScGimpProcessType.SpectrumFeed:
                    tasks.Add(ProcessSpectrumFeed());
                    break;
                case ScGimpProcessType.StoreFeed:
                    tasks.Add(ProcessStoreFeed());
                    break;
                case ScGimpProcessType.RssFeed:
                    tasks.Add(ProcessRssFeed());
                    break;
                case ScGimpProcessType.All:
                    tasks.Add(ProcessRssFeed());
                    tasks.Add(ProcessSpectrumFeed());
                    tasks.Add(ProcessStoreFeed());
                    break;
                default:
                    break;
            }

            await Task.WhenAll(tasks);

            LastUpdated = DateTimeOffset.Now;

            Processed?.Invoke(this, new ProcessedEventArgs(GetScGimpProcessType()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task ProcessSpectrumFeed()
        {
            try
            {
                await AuthorizeClientWithRsi();

                SpectrumThread.SpectrumThreadRequest[] spectrumThreadRequests = new SpectrumThread.SpectrumThreadRequest[]
                {
                    new SpectrumThread.SpectrumThreadRequest()
                    {
                        channel_id = "1",
                        label_id = null,
                        page = 1,
                        sort = "newest"
                    },
                    new SpectrumThread.SpectrumThreadRequest()
                    {
                        channel_id = "4",
                        label_id = null,
                        page = 1,
                        sort = "newest"
                    }
                };

                List<SpectrumThread> spectrumThreads = new List<SpectrumThread>();

                for (int i = 0; i < spectrumThreadRequests.Length; i++)
                {
                    try
                    {
                        SpectrumThread.SpectrumThreadRequest spectrumThreadRequest = spectrumThreadRequests[i];

                        string getThreadsJsonPayload = JsonConvert.SerializeObject(spectrumThreadRequest);
                        byte[] buffer = Encoding.UTF8.GetBytes(getThreadsJsonPayload);
                        ByteArrayContent byteContent = new ByteArrayContent(buffer);

                        using (HttpResponseMessage getThreadsResponse = await _client.PostAsync(__getThreadsUrl, byteContent, _cancellationTokenSource.Token))
                        {
                            if (!getThreadsResponse.IsSuccessStatusCode)
                            {
                                if (getThreadsResponse.StatusCode == HttpStatusCode.BadGateway)
                                {
                                    throw new RsiBadGatewayException($"HTTP post request failed for URL: {__getThreadsUrl} with status {getThreadsResponse.StatusCode.ToString()}");
                                }
                                else if (getThreadsResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
                                {
                                    throw new RsiServiceUnavailableException($"HTTP post request failed for URL: {__getThreadsUrl} with status {getThreadsResponse.StatusCode.ToString()}");
                                }
                                else
                                {
                                    throw new Exception($"HTTP post request failed for URL: {__getThreadsUrl} with status {getThreadsResponse.StatusCode.ToString()}");
                                }
                            }

                            // Read content.
                            string postResponseContent = await getThreadsResponse.Content.ReadAsStringAsync();

                            SpectrumThread.SpectrumThreadResponse spectrumThreadResponse = null;

                            try
                            {
                                spectrumThreadResponse = JsonConvert.DeserializeObject<SpectrumThread.SpectrumThreadResponse>(postResponseContent);
                            }
                            catch (TaskCanceledException) { }
                            catch (Exception ex)
                            {
                                throw new Exception("Failed to deserialize SpectrumThread.SpectrumThreadResponse. Please see inner exception for more information.", ex);
                            }

                            // Live discussion and feedback
                            if (spectrumThreadRequest.channel_id == "4")
                            {
                                var itemsToAdd = spectrumThreadResponse.data.threads
                                        .Where
                                        (
                                            t =>
                                            {
                                                string op = t?.member?.nickname?.ToLowerInvariant().TrimEnd();
                                                string subject = t?.subject?.ToLowerInvariant().TrimStart();

                                                return
                                                    !string.IsNullOrEmpty(op) &&
                                                    op.EndsWith("-cig") &&
                                                    !string.IsNullOrEmpty(subject) &&
                                                    !subject.StartsWith("test");
                                            }
                                        )
                                        .OrderByDescending(t => t.time_modified)
                                        .Select(t => SpectrumThread.FromThread(t));

                                if (itemsToAdd.Any())
                                {
                                    spectrumThreads.AddRange(itemsToAdd);
                                }
                            }
                            // Announcements
                            else
                            {
                                var itemsToAdd = spectrumThreadResponse.data.threads
                                        .Where
                                        (
                                            t => 
                                            {
                                                string subject = t?.subject?.ToLowerInvariant().TrimStart();

                                                return
                                                    !string.IsNullOrEmpty(subject) &&
                                                    !subject.StartsWith("test");
                                            }
                                        )
                                        .OrderByDescending(t => t.time_modified)
                                        .Select(t => SpectrumThread.FromThread(t));

                                if (itemsToAdd.Any())
                                {
                                    spectrumThreads.AddRange(itemsToAdd);
                                }
                            }
                        }
                    }
                    catch (RsiBadGatewayException)
                    {
                        throw;
                    }
                    catch (RsiServiceUnavailableException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to get Spectrum thread. Please see inner exception for more information.", ex);
                    }
                }

                IEqualityComparer<SpectrumThread> comparer = new SpectrumThreadEqualityComparer();

                try
                {
                    spectrumThreads = spectrumThreads.Distinct(comparer).ToList();

                    SpectrumThread[] spectrumFeedUpdates = spectrumThreads.Except(SpectrumFeed, comparer).ToArray();

                    if (SpectrumFeed.Count() == 0 &&
                        spectrumFeedUpdates.Length > 0)
                    {
                        // We just started so do not notify, just initialize for now.
                        SpectrumFeed.AddRange(spectrumFeedUpdates);

                        SpectrumFeedPoll?.Invoke(this, new SpectrumFeedPollEventArgs());
                    }
                    else if (SpectrumFeed.Count() > 0 &&
                        spectrumFeedUpdates != null &&
                        spectrumFeedUpdates.Length > 0)
                    {
                        // We have items from a previous process and we have changes.
                        SpectrumFeed.Clear();
                        SpectrumFeed.AddRange(spectrumThreads);

                        SpectrumFeedPoll?.Invoke(this, new SpectrumFeedPollEventArgs());
                        SpectrumFeedUpdate?.Invoke(this, new SpectrumFeedUpdateEventArgs(spectrumFeedUpdates));

                        await Notify(spectrumFeedUpdates);
                    }
                    else
                    {
                        SpectrumFeedPoll?.Invoke(this, new SpectrumFeedPollEventArgs());
                    }
                }
                catch (TaskCanceledException) { }
                catch (Exception ex)
                {
                    throw new Exception("Failed to handle Spectrum feed updates. Please see inner exception for more information.", ex);
                }
            }
            catch (TaskCanceledException) { }
            catch (RsiBadGatewayException) { }
            catch (RsiServiceUnavailableException) { }
            catch (Exception ex)
            {
                Error?.Invoke(this, new Exception("Failed to process the spectrum thread API feed. Please see inner exception for more information.", ex));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task ProcessStoreFeed()
        {
            try
            {
                await AuthorizeClientWithRsi();

                StoreCrawlResult crawlResult = await CrawlTheStore();

                IEqualityComparer<StoreItem> comparer = new StoreItemEqualityComparer();
                StoreItem[] storeFeedUpdates = crawlResult.StoreFeed.Except(StoreFeed, comparer).ToArray();

                if (StoreFeed.Count() == 0 &&
                    storeFeedUpdates.Length > 0)
                {
                    // We just started so do not notify, just initialize for now.
                    StoreFeed.AddRange(crawlResult.StoreFeed);

                    StoreFeedPoll?.Invoke(this, new StoreFeedPollEventArgs());
                }
                else if (StoreFeed.Count() > 0 &&
                    storeFeedUpdates != null &&
                    storeFeedUpdates.Length > 0)
                {
                    // We have items from a previous process and we have changes.
                    StoreFeed.Clear();
                    StoreFeed.AddRange(crawlResult.StoreFeed);

                    StoreFeedPoll?.Invoke(this, new StoreFeedPollEventArgs());
                    StoreFeedUpdate?.Invoke(this, new StoreFeedUpdateEventArgs(storeFeedUpdates));

                    await Notify(storeFeedUpdates);
                }
                else
                {
                    StoreFeedPoll?.Invoke(this, new StoreFeedPollEventArgs());
                }
            }
            catch (TaskCanceledException) { }
            catch (RsiBadGatewayException) { }
            catch (RsiServiceUnavailableException) { }
            catch (Exception ex)
            {
                Error?.Invoke(this, new Exception("Failed to process the store API feed. Please see inner exception for more information.", ex));
            }
        }

        

        /// <summary>
        /// Checks if the RSI token header is set and if it is not set, it acquires the token and sets it on the client.
        /// </summary>
        /// <returns></returns>
        private async Task AuthorizeClientWithRsi()
        {
            await _authorizeSemaphoreSlim.WaitAsync();

            try
            {
               string key = "x-rsi-token";

                if (!_client.DefaultRequestHeaders.Any(h => h.Key == key))
                {
                    using (HttpResponseMessage getHomeResponse = await _client.GetAsync(__homeUrl, _cancellationTokenSource.Token))
                    {
                        if (!getHomeResponse.IsSuccessStatusCode)
                        {
                            if (getHomeResponse.StatusCode == HttpStatusCode.BadGateway)
                            {
                                throw new RsiBadGatewayException($"HTTP get request failed for URL: {__homeUrl} with status {getHomeResponse.StatusCode.ToString()}");
                            }
                            if (getHomeResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                throw new RsiServiceUnavailableException($"HTTP get request failed for URL: {__homeUrl} with status {getHomeResponse.StatusCode.ToString()}");
                            }
                            else
                            {
                                throw new Exception($"HTTP get request failed for URL: {__homeUrl} with status {getHomeResponse.StatusCode.ToString()}");
                            }
                        }

                        // Prep headers so we can start querying the API.
                        Cookie rsiToken = _cookies.GetCookies(new Uri(__homeUrl)).Cast<Cookie>().FirstOrDefault(c => string.Compare(c.Name, "Rsi-Token", true) == 0);

                        _client.DefaultRequestHeaders.Add(key, rsiToken.Value);
                    }
                }
            }
            catch (RsiBadGatewayException)
            {
                throw;
            }
            catch (RsiServiceUnavailableException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to authorize client with RSI. Please see inner exception for more information.", ex);
            }
            finally
            {
                _authorizeSemaphoreSlim.Release();
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

            return storeFeedDoc.Root.Elements().Select(i => ParseStoreItem(i)).Where(i => i != null);
        }

        private StoreItem ParseStoreItem(XElement element)
        {
            StoreItem item;

            try
            {
                item = new StoreItem(element);
            }
            catch (OutOfStockStoreItemException)
            {
                item = null;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, new Exception($"Failed to parse store item. XElement:\r\n\r\n{element.ToString()}\r\n\r\nPlease see the inner exception for more information.", ex));

                item = null;
            }

            return item;
        }

        public class StoreCrawlResult
        {
            public List<StoreItem> StoreFeed { get; set; }
        }

        public class StorePageCrawlResult
        {
            public IEnumerable<StoreItem> StoreFeed { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task<StoreCrawlResult> CrawlTheStore()
        {
            StoreCrawlResult result = new StoreCrawlResult()
            {
                StoreFeed = new List<StoreItem>()
            };
            int page = 1;

            while (page >= 1 && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                StorePageCrawlResult storePage = await GetStorePage(page);

                if (storePage.StoreFeed.Count() > 0)
                {
                    result.StoreFeed.AddRange(storePage.StoreFeed);

                    page++;
                }
                else
                {
                    page = -1;
                }
            }

            page = 1;

            while (page >= 1 && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                StorePageCrawlResult storePage = await GetStorePage(page, "", "store", "", "skus", "pledge", "game-packages");

                if (storePage.StoreFeed.Count() > 0)
                {
                    result.StoreFeed.AddRange(storePage.StoreFeed);

                    page++;
                }
                else
                {
                    page = -1;
                }
            }

            page = 1;

            while (page >= 1 && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                StorePageCrawlResult storePage = await GetStorePage(page, "72", "store", "", "skus", "pledge", "extras");

                if (storePage.StoreFeed.Count() > 0)
                {
                    result.StoreFeed.AddRange(storePage.StoreFeed);

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
        /// <param name="page"></param>
        /// <param name="productId"></param>
        /// <param name="sort"></param>
        /// <param name="search"></param>
        /// <param name="itemType"></param>
        /// <param name="storeFront"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private async Task<StorePageCrawlResult> GetStorePage(int page, string productId = "", string sort = "store", string search = "", string itemType = "skus", string storeFront = "pledge", string type = "extras")
        {
            StorePageCrawlResult result = new StorePageCrawlResult()
            {
                StoreFeed = null
            };
            string getSkusJsonPayload = $"{{ \"product_id\": \"{productId}\", \"sort\": \"{sort}\", \"search\": \"{search}\", \"itemType\": \"{itemType}\", \"storefront\": \"{storeFront}\", \"type\": \"{type}\",\"page\":{page} }}";
            byte[] buffer = Encoding.UTF8.GetBytes(getSkusJsonPayload);
            ByteArrayContent byteContent = new ByteArrayContent(buffer);

            using (HttpResponseMessage getSkusResponse = await _client.PostAsync(__getSkusUrl, byteContent, _cancellationTokenSource.Token))
            {
                if (!getSkusResponse.IsSuccessStatusCode)
                {
                    if (getSkusResponse.StatusCode == HttpStatusCode.BadGateway)
                    {
                        throw new RsiBadGatewayException($"HTTP post request failed for URL: {__getSkusUrl} with status {getSkusResponse.StatusCode.ToString()}");
                    }
                    else if (getSkusResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        throw new RsiServiceUnavailableException($"HTTP post request failed for URL: {__getSkusUrl} with status {getSkusResponse.StatusCode.ToString()}");
                    }
                    else
                    {
                        throw new Exception($"HTTP post request failed for URL: {__getSkusUrl} with status {getSkusResponse.StatusCode.ToString()}");
                    }
                }

                // Read content.
                string postResponseContent = await getSkusResponse.Content.ReadAsStringAsync();

                dynamic jsonContent = JsonConvert.DeserializeObject(postResponseContent);

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
                bool bustCache = true;
                string url = bustCache ? $"{__rssUrl}?cachebuster={Guid.NewGuid().ToString("N")}" : __rssUrl;

                if (bustCache)
                {
                    ServicePoint servicePoint = ServicePointManager.FindServicePoint(new Uri(url));

                    servicePoint.ConnectionLeaseTimeout = __defaultConnectionLeaseTimeout;
                }

                using (HttpResponseMessage response = await _client.GetAsync(url, _cancellationTokenSource.Token))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.BadGateway)
                        {
                            throw new RsiBadGatewayException($"HTTP get request failed for URL: {url} with status {response.StatusCode.ToString()}");
                        }
                        else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                        {
                            throw new RsiServiceUnavailableException($"HTTP get request failed for URL: {url} with status {response.StatusCode.ToString()}");
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
                    FeedItem[] rssFeedUpdates = rssFeed
                        .Except(RssFeed, comparer)
                        // Since CIG can remove an item which then will push an old item back onto the feed which will then look as if it is new we
                        // need to check for the dates. If an item is older than yesterday then it is likely a rollback that occured.
                        .Where(i => i.PublishDate.ToUniversalTime() > DateTimeOffset.UtcNow.AddDays(-6d))
                        .ToArray();

                    if (RssFeed.Count() == 0 &&
                        rssFeedUpdates.Length > 0)
                    {
                        // We just started so do not notify, just initialize for now.
                        RssFeed.AddRange(rssFeed);

                        RssFeedPoll?.Invoke(this, new RssFeedPollEventArgs());
                    }
                    else if (
                        RssFeed.Count() > 0 &&
                        rssFeedUpdates != null &&
                        rssFeedUpdates.Length > 0)
                    {
                        // We have items from a previous process and we have changes.
                        RssFeed.Clear();
                        RssFeed.AddRange(rssFeed);

                        RssFeedPoll?.Invoke(this, new RssFeedPollEventArgs());
                        RssFeedUpdate?.Invoke(this, new RssFeedUpdateEventArgs(rssFeedUpdates));

                        await Notify(rssFeedUpdates);
                    }
                    else
                    {
                        RssFeedPoll?.Invoke(this, new RssFeedPollEventArgs());
                    }
                }
            }
            catch (TaskCanceledException) { }
            catch (RsiBadGatewayException) { }
            catch (RsiServiceUnavailableException) { }
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
            Task[] tasks = new Task[]
            {
                NotifyEmailSubscribers(updates),
                NotifyDiscordWebhooks(updates)
            };

            await Task.WhenAll(tasks);
        }

        private async Task NotifyEmailSubscribers(IEnumerable<object> updates)
        {
            try
            {
                IEnumerable<ScGimpSubscriber> subs = await _subscriberProvider.GetSubscribersAsync();

                if (!subs.Any())
                {
                    return;
                }

                Regex emailRegex = new Regex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
                SubscriberEqualityComparer comparer = new SubscriberEqualityComparer();

                subs = subs.Where(s => emailRegex.IsMatch(s.Email)).Distinct(comparer);

                List<ScGimpSubscriber> doubleVerifiedSubs = new List<ScGimpSubscriber>();

                foreach (var sub in subs)
                {
                    try
                    {
                        MailAddress address = new MailAddress(sub.Email);

                        doubleVerifiedSubs.Add(sub);
                    }
                    catch
                    {

                    }
                }

                string body = FormatEmailNotificationBody(updates);

                ScGimpNotification logEntry = new ScGimpNotification()
                {
                    Body = body.Left(4000),
                    Items = JsonConvert.SerializeObject(updates).Left(4000),
                    NotificationType = GetNotificationType(updates).Left(50),
                    Recipients = string.Join(",", doubleVerifiedSubs.Select(s => s.Id.ToString())).Left(4000),
                    Medium = "Email".Left(50)
                };

                if (IsSpam(logEntry))
                {
                    return;
                }

                using (SmtpClient client = CreateSmtpClient())
                {
                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(Options.From, "Star Citizen Gimp"),
                        IsBodyHtml = true
                    };

                    mailMessage.Bcc.Add(string.Join(",", doubleVerifiedSubs.Select(s => s.Email)));

                    mailMessage.Body = body;

                    if (updates.Count() > 1)
                    {
                        mailMessage.Subject = $"{updates.Count().ToString()} content updates detected on RSI.";
                    }
                    else if (updates.Count() == 1)
                    {
                        mailMessage.Subject = $"Content update detected on RSI.";
                    }

                    client.Send(mailMessage);

                    await _notificationLogProvider.WriteAsync(logEntry);
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Error?.Invoke(this, new Exception("Failed to notify subscribers via email. Please see inner exception for more information.", ex));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updates"></param>
        /// <returns></returns>
        private async Task NotifyDiscordWebhooks(IEnumerable<object> updates)
        {
            // Notify discord webhooks
            try
            {
                IEnumerable<ScGimpDiscordWebhook> discordWebhooks = await _discordWebhookProvider.GetDiscordWebhooksAsync();

                if (!discordWebhooks.Any())
                {
                    return;
                }

                string[] messages = FormatDiscordNotificationBody(updates);

                if (messages.Length == 0)
                {
                    return;
                }

                string body = string.Join("\\n", messages);

                ScGimpNotification logEntry = new ScGimpNotification()
                {
                    Body = body.Left(4000),
                    Items = JsonConvert.SerializeObject(updates).Left(4000),
                    NotificationType = GetNotificationType(updates).Left(50),
                    Recipients = string.Join(",", discordWebhooks.Select(wh => wh.Id.ToString())).Left(4000),
                    Medium = "Discord Webhook"
                };

                if (IsSpam(logEntry))
                {
                    return;
                }

                ScGimpDiscordWebhook[] hooks = new ScGimpDiscordWebhook[discordWebhooks.Count()];

                discordWebhooks.ToArray().CopyTo(hooks, 0);

                var parallelLoopResult = Parallel.ForEach
                (   
                    hooks, 
                    async wh =>
                    {
                        await NotifyDiscordWebhook(wh, messages);
                    }
                );

                while (!parallelLoopResult.IsCompleted)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), _cancellationTokenSource.Token);
                }

                await _notificationLogProvider.WriteAsync(logEntry);
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Error?.Invoke(this, new Exception("Failed to notify Discord Webhooks. Please see inner exception for more information.", ex));
            }
        }

        private bool IsSpam(ScGimpNotification logEntry)
        {
            IEqualityComparer<ScGimpNotification> comparer = new ScGimpNotificationEqualityComparer();

            if (NotificationHistory.Contains(logEntry, comparer))
            {
                Error?.Invoke(this, new Exception($"Spam notification of type {logEntry.NotificationType} and medium {logEntry.Medium} detected."));

                return true;
            }
            else
            {
                if (NotificationHistory.Count > 1000)
                {
                    NotificationHistory.TryDequeue(out ScGimpNotification dequeuedNotification);
                }

                NotificationHistory.Enqueue(logEntry);

                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updates"></param>
        /// <returns></returns>
        private string GetNotificationType(IEnumerable<object> updates)
        {
            string notificationType = "unknown";

            if (updates is StoreItem[])
            {
                notificationType = "Store";
            }
            else if (updates is FeedItem[])
            {
                notificationType = "Comm-link";
            }
            else if (updates is SpectrumThread[])
            {
                notificationType = "Spectrum";
            }

            return notificationType;
        }

        private async Task NotifyDiscordWebhook(ScGimpDiscordWebhook discordWebhook, string[] messages)
        {
            try
            {
                if (__discordWebhookRegex.IsMatch(discordWebhook.Url))
                {
                    string postUrl = $"{discordWebhook.Url}?wait=true";
                    ServicePoint servicePoint = ServicePointManager.FindServicePoint(new Uri(postUrl));

                    servicePoint.ConnectionLeaseTimeout = __defaultConnectionLeaseTimeout;

                    for (int i = 0; i < messages.Length; i++)
                    {
                        string message = messages[i];

                        string jsonPayload = $"{{ \"content\": \"{message}\" }}";
                        await NotifyDiscordWebhook(discordWebhook, postUrl, jsonPayload);
                    }
                }
            }
            catch (DiscordWebhookDeletedException) { }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                Error?.Invoke(this, new Exception("Failed to notify Discord Webhook. Please see inner exception for more information.", ex));
            }
        }

        private async Task NotifyDiscordWebhook(ScGimpDiscordWebhook discordWebhook, string postUrl, string jsonPayload)
        {
            TimeSpan desyncBuffer = TimeSpan.FromSeconds(1);
            bool rateLimited = false;

            using (HttpResponseMessage response = await _client.PostAsync(postUrl, new StringContent(jsonPayload, Encoding.UTF8, "application/json"), _cancellationTokenSource.Token))
            {
                response.Headers.TryGetValues("X-RateLimit-Limit", out IEnumerable<string> routeLimitHeaders);
                response.Headers.TryGetValues("X-RateLimit-Remaining", out IEnumerable<string> remainingHeaders);
                response.Headers.TryGetValues("X-RateLimit-Reset", out IEnumerable<string> resetHeaders);
                response.Headers.TryGetValues("X-RateLimit-Global", out IEnumerable<string> globalLimitHeaders);
                response.Headers.TryGetValues("Date", out IEnumerable<string> dateHeaders);


                if (dateHeaders != null &&
                    dateHeaders.FirstOrDefault() != null &&
                    DateTimeOffset.TryParse(dateHeaders.FirstOrDefault(), out DateTimeOffset now))
                {

                }
                else
                {
                    now = DateTimeOffset.Now;
                }

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == (HttpStatusCode)429)
                    {
                        rateLimited = true;
                    }
                    else if (response.StatusCode == HttpStatusCode.BadGateway)
                    {
                        Error?.Invoke(this, new RsiBadGatewayException($"Failed to notify Discord Webhook ({postUrl}). The Discord server responded with {response.StatusCode.ToString()}."));
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // The webhook has been deleted by the user so delete it from our provider.
                        await _discordWebhookProvider.DeleteAsync(discordWebhook);

                        throw new DiscordWebhookDeletedException();
                    }
                    else
                    {
                        Error?.Invoke(this, new Exception($"Failed to notify Discord Webhook ({postUrl}). The Discord server responded with {response.StatusCode.ToString()}."));
                    }
                }

                if (rateLimited)
                {
                    if
                    (
                        resetHeaders != null &&
                        resetHeaders.FirstOrDefault() != null &&
                        long.TryParse(resetHeaders.FirstOrDefault(), out long resetAt)
                    )
                    {
                        TimeSpan delay = DateTimeOffset.FromUnixTimeSeconds(resetAt) - now.ToUniversalTime() + desyncBuffer;

                        if (delay.TotalSeconds > 0d)
                        {
                            await Task.Delay(delay, _cancellationTokenSource.Token);
                        }
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromSeconds(15), _cancellationTokenSource.Token);
                    }

                    // retry
                    await NotifyDiscordWebhook(discordWebhook, postUrl, jsonPayload);
                }
                else if
                (
                    remainingHeaders != null &&
                    remainingHeaders.FirstOrDefault() != null &&
                    int.TryParse(remainingHeaders.FirstOrDefault(), out int remaining) &&
                    remaining == 0 &&
                    resetHeaders != null &&
                    resetHeaders.FirstOrDefault() != null &&
                    long.TryParse(resetHeaders.FirstOrDefault(), out long waitUntil)
                )
                {
                    TimeSpan delay = DateTimeOffset.FromUnixTimeSeconds(waitUntil) - now + desyncBuffer;

                    if (delay.TotalSeconds > 0d)
                    {
                        await Task.Delay(delay, _cancellationTokenSource.Token);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private SmtpClient CreateSmtpClient()
        {
            SmtpClient client = new SmtpClient(Options.Host);

            if (Options.Port.HasValue)
            {
                client.Port = Options.Port.Value;
            }

            if (Options.UseDefaultCredentials.HasValue)
            {
                client.UseDefaultCredentials = Options.UseDefaultCredentials.Value;
            }

            if (Options.EnableSsl.HasValue)
            {
                client.EnableSsl = Options.EnableSsl.Value;
            }

            if (Options.DeliveryMethod.HasValue)
            {
                client.DeliveryMethod = Options.DeliveryMethod.Value;
            }

            if (!string.IsNullOrWhiteSpace(Options.TargetName))
            {
                client.TargetName = Options.TargetName;
            }

            if (!string.IsNullOrWhiteSpace(Options.Username) &&
                !string.IsNullOrWhiteSpace(Options.Password) &&
                !string.IsNullOrWhiteSpace(Options.Domain))
            {
                client.Credentials = new NetworkCredential(Options.Username, Options.Password, Options.Domain); 
            } else if (!string.IsNullOrWhiteSpace(Options.Username) &&
                !string.IsNullOrWhiteSpace(Options.Password))
            {
                client.Credentials = new NetworkCredential(Options.Username, Options.Password);
            }

            return client;
        }

        private string[] FormatDiscordNotificationBody(IEnumerable<object> items)
        {
            StringBuilder builder = new StringBuilder();
            int count = 0;

            if (items.Count() > 1)
            {
                builder.AppendLine("The following content updates were detected on RSI:");
                //builder.AppendLine();

                foreach (var item in items)
                {
                    if (item is FeedItem feedItem)
                    {
                        builder.AppendLine
                        (
                            $"{(count + 1).ToString()}. {feedItem.Title} <{feedItem.Link}> was published at {feedItem.PublishDate.ToString("r")}"
                        );

                        count++;
                    }
                    else if (item is StoreItem storeItem)
                    {
                        if (!storeItem.Title.ToLowerInvariant().Contains("upgrade"))
                        {
                            builder.AppendLine
                            (
                                $"{(count + 1).ToString()}. {storeItem.Title} {storeItem.Currency} {storeItem.Cost.ToString()} <{storeItem.Url}>"
                            );

                            count++;
                        }
                    }
                    else if (item is SpectrumThread thread)
                    {
                        builder.AppendLine
                        (
                            $"{(count + 1).ToString()}. {thread.SourceThread.subject} <{thread.Url}>"
                        );

                        count++;
                    }
                }
            }
            else if (items.Count() == 1)
            {
                //builder.AppendLine("The following content update was detected on RSI:");
                //builder.AppendLine();

                var item = items.First();

                if (item is FeedItem feedItem)
                {
                    builder.AppendLine
                    (
                        $"{feedItem.Title} <{feedItem.Link}> was published at {feedItem.PublishDate.ToString("r")}"
                    );

                    count++;
                }
                else if (item is StoreItem storeItem)
                {
                    if (!storeItem.Title.ToLowerInvariant().Contains("upgrade"))
                    {
                        builder.AppendLine
                        (
                            $"{storeItem.Title} {storeItem.Currency} {storeItem.Cost.ToString()} <{storeItem.Url}>"
                        );

                        count++;
                    }
                }
                else if (item is SpectrumThread thread)
                {
                    builder.AppendLine
                    (
                        $"{thread.SourceThread.subject} <{thread.Url}>"
                    );

                    count++;
                }
            }

            //builder.AppendLine();
            //builder.AppendLine("Report a bad gimp <https://scgimp.com/Home/BadGimp>.");

            // If everything was filtered out then do not notify.
            if (count == 0)
            {
                return new string[] { };
            }
            else if (builder.Length >= 2000)
            {
                return builder.ToString().Replace("\r", string.Empty).Replace("\n", "\\n").Split("\\n", StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                return new string[] { builder.ToString().Replace("\r", string.Empty).Replace("\n", "\\n") };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        private string FormatEmailNotificationBody(IEnumerable<object> items)
        {
            StringBuilder builder = new StringBuilder();
            int count = items.Count();

            builder.AppendLine("<div>");
            builder.AppendLine("<p>Greetings Citizen,</p>");

            if (count > 1)
            {
                builder.AppendLine("<p>The following content updates were detected on RSI:</p>");

                int i = 0;

                foreach (var item in items)
                {
                    if (item is FeedItem feedItem)
                    {
                        builder.AppendLine
                        (
                            $"<div>{(i + 1).ToString()}. <a href=\"{feedItem.Link}\" alt=\"{feedItem.Title}\">{feedItem.Title}</a> was published at {feedItem.PublishDate.ToString("r")}</div>"
                        );
                    }
                    else if (item is StoreItem storeItem)
                    {
                        builder.AppendLine
                        (
                            $"<div>{(i + 1).ToString()}. <a href=\"{storeItem.Url}\" alt=\"{storeItem.Title}\">{storeItem.Title}</a> {storeItem.Currency} {storeItem.Cost.ToString()}</div>"
                        );
                    }
                    else if (item is SpectrumThread thread)
                    {
                        builder.AppendLine
                        (
                            $"<div>{(i + 1).ToString()}. <a href=\"{thread.Url}\" alt=\"{thread.SourceThread.subject}\">{thread.SourceThread.subject}</a></div>"
                        );
                    }

                    i++;
                }
            }
            else if (count == 1)
            {
                builder.AppendLine("<p>The following content update was detected on RSI:</p>");

                var item = items.First();

                if (item is FeedItem feedItem)
                {
                    builder.AppendLine
                    (
                        $"<div><a href=\"{feedItem.Link}\" alt=\"{feedItem.Title}\">{feedItem.Title}</a> was published at {feedItem.PublishDate.ToString("r")}</div>"
                    );
                }
                else if (item is StoreItem storeItem)
                {
                    builder.AppendLine
                    (
                        $"<div><a href=\"{storeItem.Url}\" alt=\"{storeItem.Title}\">{storeItem.Title}</a> {storeItem.Currency} {storeItem.Cost.ToString()}</div>"
                    );
                }
                else if (item is SpectrumThread thread)
                {
                    builder.AppendLine
                    (
                        $"<div><a href=\"{thread.Url}\" alt=\"{thread.SourceThread.subject}\">{thread.SourceThread.subject}</a></div>"
                    );
                }
            }
            builder.AppendLine("<br />");
            builder.AppendLine("<p>If you like this bot please go <a href=\"https://robertsspaceindustries.com/community/deep-space-radar/1679-Star-Citizen-Gimp\">vote</a> for it on the <a href=\"https://robertsspaceindustries.com/community/deep-space-radar/1679-Star-Citizen-Gimp\">RSI website</a>.</p>");

            builder.AppendLine("<p><a href=\"https://scgimp.com#donations\">Donate</a>&nbsp;|&nbsp;<a href=\"https://scgimp.com/Home/BadGimp\">Report a bad gimp</a>&nbsp;|&nbsp;<a href=\"https://scgimp.com/home/unsubscribe\">Unsubscribe</a></p>");

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
                            _cancellationTokenSource.Cancel();

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

                    try
                    {
                        _authorizeSemaphoreSlim.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Write(ex);
                    }

                    try
                    {
                        _cancellationTokenSource.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Write(ex);
                    }
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // NA

                // Set large fields to null.
                NotificationHistory = null;
                RssFeed = null;
                StoreFeed = null;
                SpectrumFeed = null;
                _cancellationTokenSource = null;
                _worker = null;
                _client = null;
                _handler = null;
                _cookies = null;
                _subscriberProvider = null;
                _authorizeSemaphoreSlim = null;
                _notificationLogProvider = null;
                Error -= OnError;

                disposedValue = true;
            }
        }

        // Override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // NA
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
