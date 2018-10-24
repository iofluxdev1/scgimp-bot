using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
        public async Task TestProcess()
        {
            TestSubscriberProvider provider = new TestSubscriberProvider();

            using (ScGimp gimp = new ScGimp(provider, provider, provider))
            {
                await gimp.Process();

                Assert.IsNotNull(gimp.RssFeed);
                Assert.IsTrue(gimp.RssFeed.Count() > 0);
            }
        }

        [TestMethod, Timeout(__defaultTimeout)]
        public void TestStartAndStopBeforeProcessHasCompleted()
        {
            TestSubscriberProvider provider = new TestSubscriberProvider();

            using (ScGimp gimp = new ScGimp(provider, provider, provider))
            {
                gimp.Start();

                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1d));

                gimp.Stop();
            }
        }

        [TestMethod, Timeout(__defaultTimeout)]
        public void TestStartStopAfterProcessHasRunOnce()
        {
            TestSubscriberProvider provider = new TestSubscriberProvider();

            using (ScGimp gimp = new ScGimp(provider, provider, provider))
            {
                gimp.Start();

                while (gimp.RssFeed == null || gimp.RssFeed.Count() == 0)
                {
                    System.Threading.Thread.Sleep(TimeSpan.FromMinutes(1d));
                }

                gimp.Stop();
            }
        }

        [TestMethod, Timeout(__defaultTimeout)]
        public async Task TestNotifications()
        {
            TestSubscriberProvider provider = new TestSubscriberProvider();

            using (ScGimp gimp = new ScGimp(provider, provider, provider))
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
                            PublishDate = DateTime.Now.AddDays(1d),
                            Title = "Test around the verse",
                            Type = FeedItemType.Sale
                        },
                        new FeedItem()
                        {
                            Content = "Test bugsmashers content.",
                            Element = null,
                            Link = "https://www.robertsspaceindustries.com/",
                            PublishDate = DateTime.Now.AddDays(1d),
                            Title = "Test bugsmashers",
                            Type = FeedItemType.Bugsmashers
                        }
                    }
                );

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
                        }
                    }
                );

                Assert.IsTrue(gimp.Errors.Count() == 0);
            }
        }
    }
}
