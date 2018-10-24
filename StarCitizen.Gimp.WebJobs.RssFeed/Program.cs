using Microsoft.Azure.WebJobs;
using StarCitizen.Gimp.WebJobs.Core;
using System;
using System.Threading.Tasks;

namespace StarCitizen.Gimp.WebJobs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                JobHostConfiguration config = new JobHostConfiguration();
                JobHost host = new JobHost(config);

                host.CallAsync(typeof(Functions).GetMethod("Worker"));

                host.RunAndBlock();
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                ExceptionLogger.Email(new Exception("Web job error in main. Please see inner exception for more details.", ex));
            }
        }
    }
}
