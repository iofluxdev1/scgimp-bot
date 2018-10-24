using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StarCitizen.Gimp.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
#if DEBUG
            WebHost.CreateDefaultBuilder(args)
            .UseKestrel()
            //.UseContentRoot(Directory.GetCurrentDirectory())
            .CaptureStartupErrors(true)
            .UseSetting("detailedErrors", "true")
                .UseIISIntegration()
            //.UseAzureAppServices()
                .UseApplicationInsights()
                .UseStartup<Startup>()
                .Build();
#else
        WebHost.CreateDefaultBuilder(args)
            //.UseKestrel()
            //.UseContentRoot(Directory.GetCurrentDirectory())
            //.CaptureStartupErrors(true)
            //.UseSetting("detailedErrors", "true")
                //.UseIISIntegration()
                .UseAzureAppServices()
                .UseApplicationInsights()
                .UseStartup<Startup>()
                .Build();
#endif
    }
}
