using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace StarCitizen.Gimp.Core
{
    /// <summary>
    /// 
    /// </summary>
    public struct ScGimpOptions
    {
        /// <summary>
        /// The timespan to sleep for inbetween polling.
        /// </summary>
        public TimeSpan Sleep { get; set; }

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
        /// SMTP target name.
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// The SMTP from address.
        /// </summary>
        public string From { get; set; }
    }
}
