using System;
using System.Net.Mail;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class ScGimpOptions
    {
        public TimeSpan Sleep { get; set; }

        public double AfterHoursSleepMultiplier { get; set; }

        public double OutsideCommLinkSleepMultiplier { get; set; }

        public TimeSpan CigWorkingHoursStart { get; set; }

        public TimeSpan CigWorkingHoursEnd { get; set; }

        public TimeSpan CigCommLinkEnd { get; set; }

        public TimeSpan CigCommLinkStart { get; set; }

        /// <summary>
        /// SMTP host.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// SMTP port.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// SMTP use default credentials.
        /// </summary>
        public bool? UseDefaultCredentials { get; set; }

        /// <summary>
        /// SMTP username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// SMTP password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// SMTP domain.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// SMTP enable SSL.
        /// </summary>
        public bool? EnableSsl { get; set; }

        /// <summary>
        /// SMTP delivery method.
        /// </summary>
        public SmtpDeliveryMethod? DeliveryMethod { get; set; }

        /// <summary>
        /// The optional process type. If no process type is specified all types will be monitored.
        /// </summary>
        public ScGimpProcessType? ProcessType { get; set; }

        /// <summary>
        /// SMTP target name.
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// The SMTP from address.
        /// </summary>
        public string From { get; set; }

        public ScGimpOptions()
        {
            ProcessType = null;
            DeliveryMethod = null;
            Domain = null;
            EnableSsl = null;
            From = null;
            Host = null;
            Password = null;
            Port = null;
            Sleep = TimeSpan.FromSeconds(1d);
            TargetName = null;
            UseDefaultCredentials = null;
            Username = null;
            AfterHoursSleepMultiplier = 1200d;
            CigWorkingHoursEnd = TimeSpan.FromHours(18d);
            CigWorkingHoursStart = TimeSpan.FromHours(8d);
            CigCommLinkEnd = TimeSpan.FromHours(14d);
            CigCommLinkStart = TimeSpan.FromHours(11d);
            OutsideCommLinkSleepMultiplier = 120d;
        }
    }
}
