using StarCitizen.Gimp.Core;
using System;
using System.Linq;

namespace StarCitizen.Gimp.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            System.Console.WriteLine("==========================");
            System.Console.WriteLine("| Star Citizen Gimp v1.0 |");
            System.Console.WriteLine("==========================");

            try
            {
                DummySubscriberProvider provider = new DummySubscriberProvider();

                using (ScGimp gimp = new ScGimp(provider, provider, provider))
                {
                    gimp.RssFeedUpdate += new RssFeedUpdateEventHandler(OnRssFeedUpdate);
                    gimp.RssFeedPoll += new RssFeedPollEventHandler(OnRssFeedPoll);
                    gimp.Error += new ErrorEventHandler(OnError);
                    gimp.StoreFeedPoll += new StoreFeedPollEventHandler(OnStoreFeedPoll);
                    gimp.StoreFeedUpdate += new StoreFeedUpdateEventHandler(OnStoreFeedUpdate);

                    gimp.Start();

                    while (System.Console.ReadKey().Key != ConsoleKey.X)
                    {
                        Print("Press X to stop.");
                    }
                }

                Print("Star Citizen Gimp has exited.");
            }
            catch (Exception ex)
            {
                Print("Error");
                System.Console.WriteLine(ex.ToString());
            }
        }

        private static void OnStoreFeedUpdate(object sender, StoreFeedUpdateEventArgs e)
        {
            Print($"The store feed has been updated ({e.Items.Count()} total store items) and subscribers notified.", e);
        }

        private static void OnStoreFeedPoll(object sender, StoreFeedPollEventArgs e)
        {
            Print("No store feed changes detected.", e);
        }

        private static void OnError(object sender, Exception ex)
        {
            Print("Error");
            System.Console.WriteLine(ex.ToString());
        }

        private static void OnRssFeedPoll(object sender, RssFeedPollEventArgs e)
        {
            Print("No RSS feed changes detected.", e);
        }

        private static void OnRssFeedUpdate(object sender, RssFeedUpdateEventArgs e)
        {
            Print($"The RSS feed has been updated ({e.Items.Count()} total feed items) and subscribers notified.", e);
        }

        private static void Print(string message, BandwidthEventArgs e = null)
        {
            System.Console.WriteLine($"{DateTimeOffset.Now.ToString("g")}: {message}");
            
            if (e != null)
            {
                System.Console.WriteLine($"{DateTimeOffset.Now.ToString("g")}: Downloaded {FormatSize((ulong)e.ContentLength)} with a total of {FormatSize((ulong)e.TotalContentLength)}");
            }
        }

        private static readonly string[] UNITS = new string[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

        public static string FormatSize(ulong bytes)
        {
            int c = 0;
            for (c = 0; c < UNITS.Length; c++)
            {
                ulong m = (ulong)1 << ((c + 1) * 10);
                if (bytes < m)
                    break;
            }

            double n = bytes / (double)((ulong)1 << (c * 10));
            return string.Format("{0:0.##} {1}", n, UNITS[c]);
        }
    }
}
