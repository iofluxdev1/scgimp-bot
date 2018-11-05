using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Mail;

namespace StarCitizen.Gimp.Core
{
    public class ScGimpCoreConfig
    {
        public ScGimpOptions Options { get; set; }

        public IConfigurationRoot Configuration { get; set; }

        public ScGimpCoreConfig()
        {
            Initialize(null);
        }

        public ScGimpCoreConfig(IConfigurationBuilder configBuilder)
        {
            Initialize(configBuilder);
        }

        private void Initialize(IConfigurationBuilder configBuilder)
        {
            if (configBuilder == null)
            {
                configBuilder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true, reloadOnChange: true)
                    .AddJsonFile("appsettings.production.json", true, reloadOnChange: true)
                    .AddEnvironmentVariables();
            }

            Configuration = configBuilder.Build();

            Options = new ScGimpOptions()
            {
                ProcessType = Configuration["ProcessType"] == "RssFeed" ?
                    ScGimpProcessType.RssFeed :
                    (
                            Configuration["ProcessType"] == "SpectrumFeed" ?
                                ScGimpProcessType.SpectrumFeed :
                                (
                                    Configuration["ProcessType"] == "StoreFeed" ?
                                    ScGimpProcessType.StoreFeed :
                                    (
                                        Configuration["ProcessType"] == "All" ?
                                        ScGimpProcessType.All :
                                        (ScGimpProcessType?)null
                                    )
                                )
                    ),
                DeliveryMethod = Configuration["SmtpDeliveryMethod"] == "Network" ?
                    SmtpDeliveryMethod.Network :
                    (
                            Configuration["SmtpDeliveryMethod"] == "PickupDirectoryFromIis" ?
                                SmtpDeliveryMethod.PickupDirectoryFromIis :
                                (
                                    Configuration["SmtpDeliveryMethod"] == "SpecifiedPickupDirectory" ?
                                    SmtpDeliveryMethod.SpecifiedPickupDirectory :
                                    (SmtpDeliveryMethod?)null
                                )
                    ),
                Domain = string.IsNullOrWhiteSpace(Configuration["SmtpDomain"]) ? null : Configuration["SmtpDomain"],
                EnableSsl = string.IsNullOrWhiteSpace(Configuration["SmtpEnableSsl"]) ? (bool?)null : bool.Parse(Configuration["SmtpEnableSsl"]),
                From = string.IsNullOrWhiteSpace(Configuration["SmtpFrom"]) ? null : Configuration["SmtpFrom"],
                Host = string.IsNullOrWhiteSpace(Configuration["SmtpHost"]) ? null : Configuration["SmtpHost"],
                Password = string.IsNullOrWhiteSpace(Configuration["SmtpPassword"]) ? null : Configuration["SmtpPassword"],
                Port = string.IsNullOrWhiteSpace(Configuration["SmtpPort"]) ? (int?)null : int.Parse(Configuration["SmtpPort"]),
                Sleep = string.IsNullOrWhiteSpace(Configuration["Sleep"]) ? TimeSpan.FromSeconds(1d) : TimeSpan.FromSeconds(double.Parse(Configuration["Sleep"])),
                TargetName = string.IsNullOrWhiteSpace(Configuration["SmtpTargetName"]) ? null : Configuration["SmtpTargetName"],
                UseDefaultCredentials = string.IsNullOrWhiteSpace(Configuration["SmtpUseDefaultCredentials"]) ? (bool?)null : bool.Parse(Configuration["SmtpUseDefaultCredentials"]),
                Username = string.IsNullOrWhiteSpace(Configuration["SmtpUsername"]) ? null : Configuration["SmtpUsername"],
                AfterHoursSleepMultiplier = string.IsNullOrWhiteSpace(Configuration["AfterHoursSleepMultiplier"]) ? 1200d : double.Parse(Configuration["AfterHoursSleepMultiplier"]),
                CigWorkingHoursEnd = string.IsNullOrWhiteSpace(Configuration["CigWorkingHoursEnd"]) ? TimeSpan.FromHours(18d) : TimeSpan.FromHours(double.Parse(Configuration["CigWorkingHoursEnd"])),
                CigWorkingHoursStart = string.IsNullOrWhiteSpace(Configuration["CigWorkingHoursStart"]) ? TimeSpan.FromHours(8d) : TimeSpan.FromHours(double.Parse(Configuration["CigWorkingHoursStart"])),
                CigCommLinkEnd = string.IsNullOrWhiteSpace(Configuration["CigCommLinkEnd"]) ? TimeSpan.FromHours(14d) : TimeSpan.FromHours(double.Parse(Configuration["CigCommLinkEnd"])),
                CigCommLinkStart = string.IsNullOrWhiteSpace(Configuration["CigCommLinkStart"]) ? TimeSpan.FromHours(11d) : TimeSpan.FromHours(double.Parse(Configuration["CigCommLinkStart"])),
                OutsideCommLinkSleepMultiplier = string.IsNullOrWhiteSpace(Configuration["OutsideCommLinkSleepMultiplier"]) ? 120d : double.Parse(Configuration["OutsideCommLinkSleepMultiplier"])
            };
        }
    }
}
