using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;

namespace StarCitizen.Gimp.Web
{
    public class KuduWebJobService
    {
        public string KuduUserName { get; private set; }
        public string KuduPassword { get; private set; }
        public string AzureWebsiteName { get; private set; }

        public KuduWebJobService(string kuduUserName, string kuduPassword, string azureWebsiteName)
        {
            if (string.IsNullOrWhiteSpace(kuduUserName))
            {
                throw new ArgumentNullException("kuduUserName");
            }

            if (string.IsNullOrWhiteSpace(kuduPassword))
            {
                throw new ArgumentNullException("kuduPassword");
            }

            if (string.IsNullOrWhiteSpace(azureWebsiteName))
            {
                throw new ArgumentNullException("azureWebsiteName");
            }

            KuduUserName = kuduUserName;
            KuduPassword = kuduPassword;
            AzureWebsiteName = azureWebsiteName;
        }

        public string GetWebJobStatus(string webJobName)
        {
            var status = "Unknown";

            using (var client = CreateWebClient())
            {
                var jobJson = client.DownloadString($"https://{AzureWebsiteName}.scm.azurewebsites.net/api/continuouswebjobs/scgimp");

                dynamic job = JsonConvert.DeserializeObject(jobJson);

                status = job["status"];
            }

            return status;
        }

        public string StartWebJob(string webJobName)
        {
            var response = string.Empty;

            using (var client = CreateWebClient())
            {
                response = client.UploadString($"https://{AzureWebsiteName}.scm.azurewebsites.net/api/continuouswebjobs/{webJobName}/start", "POST", "");
            }

            return response;
        }

        public string StopWebJob(string webJobName)
        {
            string response = string.Empty;

            using (var client = CreateWebClient())
            {
                response = client.UploadString($"https://{AzureWebsiteName}.scm.azurewebsites.net/api/continuouswebjobs/{webJobName}/stop", "POST", "");
            }

            return response;
        }

        private WebClient CreateWebClient()
        {
            var client = new WebClient();
            var base64Auth = Convert.ToBase64String(Encoding.Default.GetBytes($"{KuduUserName}:{KuduPassword}"));
            client.Headers.Add(HttpRequestHeader.Authorization, $"Basic {base64Auth}");

            return client;
        }
    }
}
