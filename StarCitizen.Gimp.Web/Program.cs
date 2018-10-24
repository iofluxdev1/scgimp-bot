using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace StarCitizen.Gimp.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IWebHostBuilder webHostBuilder = CreateWebHostBuilder(args)
                .UseStartup<Startup>();

            using (IWebHost webHost = webHostBuilder.Build())
            {
                webHost.Run();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                // https://github.com/aspnet/MetaPackages/blob/rel/2.0.0-preview1/src/Microsoft.AspNetCore/WebHost.cs
                return WebHost.CreateDefaultBuilder(args)
                    .CaptureStartupErrors(true)
                    .UseSetting("detailedErrors", "true")
                    .UseApplicationInsights();
            }
            else if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                // https://github.com/aspnet/MetaPackages/blob/rel/2.0.0-preview1/src/Microsoft.AspNetCore/WebHost.cs
                return WebHost.CreateDefaultBuilder(args)
                    .UseAzureAppServices()
                    .UseApplicationInsights();
            }
            else
            {
                // https://github.com/aspnet/MetaPackages/blob/rel/2.0.0-preview1/src/Microsoft.AspNetCore/WebHost.cs
                return WebHost.CreateDefaultBuilder(args)
                    .CaptureStartupErrors(true)
                    .UseSetting("detailedErrors", "true")
                    .UseApplicationInsights();
            }
        }
    }
}
