using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace StarCitizen.Gimp.WebJobs
{
    public static class ExceptionLogger
    {
        public static void Log(Exception ex)
        {
            try
            {
                IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");

                IConfigurationRoot configuration = configBuilder.Build();

                using (SmtpClient client = CreateSmtpClient(configuration))
                {
                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(configuration["From"], "Star Citizen Gimp"),
                        IsBodyHtml = false
                    };

                    // TODO add to config and inject
                    mailMessage.To.Add(new MailAddress("changetoyourownvalidemail@hotmail.co.uk"));

                    mailMessage.Body = FormatErrorBody(ex);
                    mailMessage.Subject = "Star Citizen Gimp web job error.";

                    client.Send(mailMessage);
                }
            }
            catch (Exception mailEx)
            {
                System.Diagnostics.Trace.WriteLine(mailEx.ToString());
            }
        }

        private static string FormatErrorBody(Exception ex)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("Error Message:");
            builder.AppendLine(ex.Message);
            builder.AppendLine();
            builder.AppendLine("Exception:");
            builder.AppendLine(ex.ToString());

            return builder.ToString();
        }

        private static SmtpClient CreateSmtpClient(IConfigurationRoot configuration)
        {
            SmtpClient client = new SmtpClient(configuration["Host"])
            {
                Port = int.Parse(configuration["Port"]),
                Credentials = new NetworkCredential(configuration["Username"], configuration["Password"])
            };

            return client;
        }
    }
}
