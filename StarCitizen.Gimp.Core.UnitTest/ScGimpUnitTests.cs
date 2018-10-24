using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StarCitizen.Gimp.Core.UnitTest
{
    [TestClass]
    public class ScGimpUnitTests
    {
        /// <summary>
        /// The default test timeout in milliseconds.
        /// </summary>
        private const int __defaultTimeout = 180000; // 3 minutes

        [TestMethod, Timeout(__defaultTimeout)]
        public void TestConstructor()
        {
            TestSubscriberProvider provider = new TestSubscriberProvider();

            using (ScGimp gimp = new ScGimp(provider, provider, provider))
            {
                Assert.IsNotNull(gimp);
            }
        }

        [TestMethod, Timeout(__defaultTimeout)]
        public async Task TestProcessRss()
        {
            TestSubscriberProvider provider = new TestSubscriberProvider();
            ScGimpCoreConfig config = new ScGimpCoreConfig();

            config.Options.ProcessType = ScGimpProcessType.RssFeed;

            using (ScGimp gimp = new ScGimp(provider, provider, provider, config.Options))
            {
                await gimp.Process();

                Assert.IsNotNull(gimp.RssFeed);
                Assert.IsTrue(gimp.RssFeed.Count() > 0);
            }
        }

        [TestMethod, Timeout(__defaultTimeout)]
        public async Task TestProcessSpectrum()
        {
            TestSubscriberProvider provider = new TestSubscriberProvider();
            ScGimpCoreConfig config = new ScGimpCoreConfig();

            config.Options.ProcessType = ScGimpProcessType.SpectrumFeed;

            using (ScGimp gimp = new ScGimp(provider, provider, provider, config.Options))
            {
                await gimp.Process();

                Assert.IsNotNull(gimp.SpectrumFeed);
                Assert.IsTrue(gimp.SpectrumFeed.Count() > 0);
            }
        }

        [TestMethod, Timeout(__defaultTimeout)]
        public async Task TestProcessStore()
        {
            return;

            TestSubscriberProvider provider = new TestSubscriberProvider();
            ScGimpCoreConfig config = new ScGimpCoreConfig();

            config.Options.ProcessType = ScGimpProcessType.StoreFeed;

            using (ScGimp gimp = new ScGimp(provider, provider, provider, config.Options))
            {
                await gimp.Process();

                Assert.IsNotNull(gimp.StoreFeed);
                Assert.IsTrue(gimp.StoreFeed.Count() > 0);
            }
        }

        [TestMethod, Timeout(__defaultTimeout)]
        public async Task TestStartAndStop()
        {
            TestSubscriberProvider provider = new TestSubscriberProvider();
            ScGimpCoreConfig config = new ScGimpCoreConfig();

            config.Options.ProcessType = ScGimpProcessType.RssFeed;

            using (ScGimp gimp = new ScGimp(provider, provider, provider, config.Options))
            {
                await gimp.Start();

                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1d));

                await gimp.Stop();
            }
        }

        [TestMethod, Timeout(__defaultTimeout)]
        public async Task TestStoreFeedNotifications()
        {
            TestSubscriberProvider provider = new TestSubscriberProvider();
            ScGimpCoreConfig config = new ScGimpCoreConfig();

            config.Options.ProcessType = ScGimpProcessType.StoreFeed;

            using (ScGimp gimp = new ScGimp(provider, provider, provider, config.Options))
            {
                await gimp.Notify
                (
                    new StoreItem[]
                    {
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 200.50m,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store warbond dingy LTI",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package",
                            Type = StoreItemType.Unknown
                        }
                    }
                );

                Assert.IsTrue(gimp.Errors.Count() == 0);
            }
        }

        [TestMethod, Timeout(__defaultTimeout)]
        public async Task TestNoCcuNotificationsForDiscordWebhooks()
        {
            TestSubscriberProvider provider = new TestSubscriberProvider();
            ScGimpCoreConfig config = new ScGimpCoreConfig();

            config.Options.ProcessType = ScGimpProcessType.StoreFeed;

            using (ScGimp gimp = new ScGimp(provider, provider, provider, config.Options))
            {
                await gimp.Notify
                (
                    new StoreItem[]
                    {
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 200.50m,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store warbond dingy LTI upgrade",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float package Upgrade",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = " upgrade Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Upgrade Test store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test UPGRADE store float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store upgrades float package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float Upgrades package",
                            Type = StoreItemType.Unknown
                        },
                        new StoreItem()
                        {
                            Element = null,
                            Cost = 45,
                            Url = "https://www.robertsspaceindustries.com/",
                            Currency = "USD",
                            Title = "Test store float UPGRADES package",
                            Type = StoreItemType.Unknown
                        }
                    }
                );

                Assert.IsTrue(gimp.Errors.Count() == 0);
            }
        }

        [TestMethod, Timeout(__defaultTimeout)]
        public async Task TestSpectrumFeedNotifications()
        {
            TestSubscriberProvider provider = new TestSubscriberProvider();
            ScGimpCoreConfig config = new ScGimpCoreConfig();

            config.Options.ProcessType = ScGimpProcessType.SpectrumFeed;

            using (ScGimp gimp = new ScGimp(provider, provider, provider, config.Options))
            {
                await gimp.Notify
                (
                    new SpectrumThread[]
                    {
                        SpectrumThread.FromThread
                        (
                            new SpectrumThread.Thread()
                            {
                                id = "1",
                                subject = "Test announcement Publish to LIVE!",
                                slug = "test-announcement-publish-to-live",
                                channel_id = "1",
                                time_created = (int)DateTimeOffset.Now.AddDays(-1).ToUnixTimeMilliseconds()
                            }
                        ),
                        SpectrumThread.FromThread
                        (
                            new SpectrumThread.Thread()
                            {
                                id = "2",
                                subject = "Test announcement Release notes",
                                slug = "test-announcement-release-notes",
                                channel_id = "1",
                                time_created = (int)DateTimeOffset.Now.AddDays(-2).ToUnixTimeMilliseconds()
                            }
                        )
                    }
                );

                await Task.Delay(TimeSpan.FromSeconds(60));

                Assert.IsTrue(gimp.Errors.Count() == 0);
            }
        }


        [TestMethod, Timeout(__defaultTimeout)]
        public async Task TestRssFeedNotifications()
        {
            TestSubscriberProvider provider = new TestSubscriberProvider();
            ScGimpCoreConfig config = new ScGimpCoreConfig();

            config.Options.ProcessType = ScGimpProcessType.RssFeed;

            using (ScGimp gimp = new ScGimp(provider, provider, provider, config.Options))
            {
                await gimp.Notify
                (
                    new FeedItem[]
                    {
                        new FeedItem()
                        {
                            Content = "Test around the verse content.",
                            Element = null,
                            Link = "https://www.robertsspaceindustries.com/",
                            PublishDate = DateTimeOffset.Now.AddDays(1d),
                            Title = "Test around the verse",
                            Type = FeedItemType.Sale
                        },
                        new FeedItem()
                        {
                            Content = "Test bugsmashers content.",
                            Element = null,
                            Link = "https://www.robertsspaceindustries.com/",
                            PublishDate = DateTimeOffset.Now.AddDays(1d),
                            Title = "Test bugsmashers",
                            Type = FeedItemType.Bugsmashers
                        }
                    }
                );

                Assert.IsTrue(gimp.Errors.Count() == 0);
            }
        }

        [TestMethod, Timeout(__defaultTimeout)]
        public async Task TestAntiSpam()
        {
            TestSubscriberProvider provider = new TestSubscriberProvider();
            ScGimpCoreConfig config = new ScGimpCoreConfig();
            FeedItem[] items = new FeedItem[]
            {
                new FeedItem()
                {
                    Content = "Test around the verse content.",
                    Element = null,
                    Link = "https://www.robertsspaceindustries.com/",
                    PublishDate = DateTimeOffset.Now.AddDays(1d),
                    Title = "Test around the verse",
                    Type = FeedItemType.Sale
                },
                new FeedItem()
                {
                    Content = "Test bugsmashers content.",
                    Element = null,
                    Link = "https://www.robertsspaceindustries.com/",
                    PublishDate = DateTimeOffset.Now.AddDays(1d),
                    Title = "Test bugsmashers",
                    Type = FeedItemType.Bugsmashers
                }
            };

            config.Options.ProcessType = ScGimpProcessType.RssFeed;

            using (ScGimp gimp = new ScGimp(provider, provider, provider, config.Options))
            {
                await gimp.Notify(items);

                await Task.Delay(TimeSpan.FromSeconds(5));

                await gimp.Notify(items);

                Assert.IsTrue(gimp.Errors.Where(e => !e.Message.Contains("Spam")).Count() == 0);
            }
        }
    }
}
