using StarCitizen.Gimp.WebJobs.Core;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarCitizen.Gimp.WebJobs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
                {
                    void cancelKeyPressEventHandler(object sender, ConsoleCancelEventArgs eventArgs)
                    {
                        cancellationTokenSource.Cancel();
                    }

                    Console.CancelKeyPress += cancelKeyPressEventHandler;
                    Task task = null;

                    try
                    {
                        task = Task.Run
                        (
                            async () =>
                            {
                                while (true)
                                {
                                    StringBuilder stringBuilder = new StringBuilder();
                                    using (StringWriter stringWriter = new StringWriter(stringBuilder))
                                    {
                                        await Functions.Worker(stringWriter, cancellationTokenSource.Token);
                                    }
                                }
                            },
                            cancellationTokenSource.Token
                        );

                        task.Wait(cancellationTokenSource.Token);
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        Console.CancelKeyPress -= cancelKeyPressEventHandler;

                        if (task != null)
                        {
                            task.Dispose();
                        }
                    }
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                ExceptionLogger.Email(new Exception("Error in main. Please see inner exception for more details.", ex));
            }
        }
    }
}
