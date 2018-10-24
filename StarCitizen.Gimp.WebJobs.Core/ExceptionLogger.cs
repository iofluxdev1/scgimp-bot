using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace StarCitizen.Gimp.WebJobs.Core
{
    public static class ExceptionLogger
    {
        private static uint __errorCount = 0;

        public static void Email(Exception ex)
        {
            __errorCount++;

            if (__errorCount > 10) {
                return;
            }

            try
            {
                ScGimpWebJobConfig config = new ScGimpWebJobConfig();

                using (SmtpClient client = CreateSmtpClient(config))
                {
                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(config.Options.From, "Star Citizen Gimp"),
                        IsBodyHtml = false
                    };

                    mailMessage.To.Add(config.TraceRecipients);

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

        private static SmtpClient CreateSmtpClient(ScGimpWebJobConfig config)
        {
            SmtpClient client = new SmtpClient(config.Options.Host)
            {
                Port = config.Options.Port ?? 587,
                Credentials = new NetworkCredential(config.Options.Username, config.Options.Password)
            };

            return client;
        }
    }
}
