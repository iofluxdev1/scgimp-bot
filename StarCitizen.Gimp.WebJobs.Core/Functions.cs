using StarCitizen.Gimp.Core;
using StarCitizen.Gimp.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StarCitizen.Gimp.WebJobs.Core
{
    public class Functions
    {
        private static TextWriter _log;
        private static bool _debug;

        public static async Task Worker(TextWriter log, CancellationToken token)
        {
            try
            {
                _log = log;

                ScGimpWebJobConfig config = new ScGimpWebJobConfig();

                _debug = config.Debug;

                object provider = null;

                if (config.Debug)
                {
                    provider = new DummySubscriberProvider();

                    await log.WriteLineAsync($"WARNING: Debug mode is enabled.");
                }
                else
                {
                    provider = new DbProvider();
                }

                await log.WriteLineAsync($"Gimp subscriber provider: {provider.GetType().FullName}");
                await log.WriteLineAsync($"Gimp discord webhook provider: {provider.GetType().FullName}");
                await log.WriteLineAsync($"Gimp notification log provider: {provider.GetType().FullName}");

                using
                (
                    ScGimp gimp = new ScGimp
                    (
                        (ISubscriberProvider)provider,
                        (IDiscordWebhookProvider)provider,
                        (INotificationLogProvider)provider,
                        config.Options
                    )
                )
                {
                    gimp.Error += OnError;
                    gimp.Processing += OnProcessing;
                    gimp.Processed += OnProcessed;
                    gimp.RssFeedUpdate += OnRssFeedUpdate;
                    gimp.SpectrumFeedUpdate += OnSpectrumFeedUpdate;
                    gimp.StoreFeedUpdate += OnStoreFeedUpdate;

                    await log.WriteLineAsync($"Gimp process type: {Enum.GetName(typeof(ScGimpProcessType), gimp.GetScGimpProcessType())}");
                    await log.WriteLineAsync($"Gimp default sleep: {gimp.Options.Sleep.ToString()}");
                    await log.WriteLineAsync($"Gimp after hours sleep multiplier: {gimp.Options.AfterHoursSleepMultiplier.ToString()}");
                    await log.WriteLineAsync($"Gimp outside comm link sleep multiplier: {gimp.Options.OutsideCommLinkSleepMultiplier.ToString()}");
                    await log.WriteLineAsync($"CIG comm link end: {gimp.Options.CigCommLinkEnd.ToString()}");
                    await log.WriteLineAsync($"CIG comm link start: {gimp.Options.CigCommLinkStart.ToString()}");
                    await log.WriteLineAsync($"CIG working hours end: {gimp.Options.CigWorkingHoursEnd.ToString()}");
                    await log.WriteLineAsync($"CIG working hours start: {gimp.Options.CigWorkingHoursStart.ToString()}");

                    await log.WriteLineAsync($"Starting the gimp.");

                    await gimp.Start();

                    await log.WriteLineAsync($"The gimp has started.");

                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            // auto restart if not running.
                            if
                            (
                                gimp.Status == null ||
                                gimp.Status.Value == TaskStatus.Faulted ||
                                gimp.Status.Value == TaskStatus.Canceled ||
                                gimp.Status.Value == TaskStatus.RanToCompletion ||
                                (
                                    gimp.LastUpdated.HasValue &&
                                    gimp.LastUpdated.Value.ToUniversalTime() < DateTimeOffset.UtcNow.AddHours(-1d)
                                )
                            )
                            {
                                await log.WriteLineAsync($"Restarting the gimp.");

                                await gimp.Start();

                                await log.WriteLineAsync($"The gimp has restarted.");
                            }

                            await Task.Delay(TimeSpan.FromMinutes(5), token);
                        }
                        catch (TaskCanceledException) { }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    }

                    gimp.Processing -= OnProcessing;
                    gimp.Processed -= OnProcessed;
                    gimp.RssFeedUpdate -= OnRssFeedUpdate;
                    gimp.SpectrumFeedUpdate -= OnSpectrumFeedUpdate;
                    gimp.StoreFeedUpdate -= OnStoreFeedUpdate;
                    gimp.Error -= OnError;

                    await log.WriteLineAsync($"Stopping the gimp.");

                    await gimp.Stop();

                    await log.WriteLineAsync($"The gimp has stopped.");
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception exception)
            {
                LogException(exception);
            }
        }

        private static async void OnStoreFeedUpdate(object sender, StoreFeedUpdateEventArgs e)
        {
            try
            {
                await _log.WriteLineAsync($"{e.Items.Count().ToString()} new RSI store feed update{(e.Items.Count() == 1 ? string.Empty : "s")} {(e.Items.Count() > 1 ? "have" : "has")} been detected.");
            }
            catch (Exception)
            {
                
            }
        }

        private static async void OnSpectrumFeedUpdate(object sender, SpectrumFeedUpdateEventArgs e)
        {
            try
            {
                await _log.WriteLineAsync($"{e.Items.Count().ToString()} new Spectrum post{(e.Items.Count() == 1 ? string.Empty : "s")} {(e.Items.Count() > 1 ? "have" : "has")} been detected.");
            }
            catch (Exception)
            {

            }
        }

        private static async void OnRssFeedUpdate(object sender, RssFeedUpdateEventArgs e)
        {
            try
            {
                await _log.WriteLineAsync($"{e.Items.Count().ToString()} new Comm-link article{(e.Items.Count() == 1 ? string.Empty : "s")} {(e.Items.Count() > 1 ? "have" : "has")} been detected.");
            }
            catch (Exception)
            {

            }
        }

        private static async void OnProcessed(object sender, ProcessedEventArgs e)
        {
            try
            {
                if (_debug)
                {
                    await _log.WriteLineAsync($"Processing completed.");
                }
            }
            catch (Exception)
            {

            }
        }

        private static async void OnProcessing(object sender, ProcessingEventArgs e)
        {
            try
            {
                if (_debug)
                {
                    await _log.WriteLineAsync($"Processing started.");
                }
            }
            catch (Exception)
            {

            }
        }

        private static void OnError(object sender, Exception ex)
        {
            LogException(ex);
        }

        public static async Task LogAsync(string message)
        {
            try
            {
                await _log.WriteLineAsync(message);
            }
            catch (Exception ex)
            {
                ExceptionLogger.Email(new Exception(message));
                ExceptionLogger.Email(ex);
            }
        }

        public static void LogException(Exception ex)
        {
            if (_log != null)
            {
                try
                {
                    _log.WriteLine("Error:");
                    _log.WriteLine(ex.ToString());
                }
                catch (Exception logEx)
                {
                    ExceptionLogger.Email(logEx);
                }
            }

            ExceptionLogger.Email(ex);
        }
    }
}
