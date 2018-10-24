using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using StarCitizen.Gimp.Core;
using StarCitizen.Gimp.Data;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarCitizen.Gimp.WebJobs
{
    public class Program
    {
        public static ScGimp Gimp { get; private set; }

        public static void Main(string[] args)
        {
            try
            {
                IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");
                IConfigurationRoot configuration = configBuilder.Build();
                JobHostConfiguration config = new JobHostConfiguration();
                JobHost host = new JobHost(config);
                DbProvider provider = new DbProvider();
                using (Gimp = new ScGimp(provider, provider, provider))
                {
                    Gimp.Start();

                    Gimp.Error += new Core.ErrorEventHandler((s, e) => ExceptionLogger.Log(e));

                    host.CallAsync(typeof(Functions).GetMethod("KeepAlive"));

                    host.RunAndBlock();
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.Log(new Exception("Web job error in main. Please see inner exception for more details.", ex));
            }
        }
    }
}
