using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarCitizen.Gimp.WebJobs
{
    public class Functions
    {
        [NoAutomaticTrigger]
        public static async Task KeepAlive(TextWriter log, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        if (Program.Gimp != null)
                        {
                            await log.WriteLineAsync($"Keep alive: {Program.Gimp?.Errors?.Count} total errors, {Program.Gimp?.StoreFeed?.Count} total store feed items and {Program.Gimp?.RssFeed?.Count} total RSS feed items.");
                        }

                        await Task.Delay(TimeSpan.FromHours(1));
                    }
                    catch (Exception ex)
                    {
                        LogException(log, ex);
                    }
                }
            }
            catch (Exception exception)
            {
                LogException(log, exception);
            }
        }

        private static void LogException(TextWriter log, Exception ex)
        {
            if (log != null)
            {
                try
                {
                    log.WriteLine("Error:");
                    log.WriteLine(ex.ToString());
                }
                catch (Exception logEx)
                {
                    ExceptionLogger.Log(ex);
                    ExceptionLogger.Log(logEx);
                }
            }
            else
            {
                ExceptionLogger.Log(ex);
            }
        }
    }
}
