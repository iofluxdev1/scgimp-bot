using Microsoft.Extensions.Configuration;
using StarCitizen.Gimp.Core;

namespace StarCitizen.Gimp.Web
{
    public class ScGimpWebConfig : ScGimpCoreConfig
    {
        public string KuduUserName { get; set; }

        public string KuduPassword { get; set; }

        public string AzureWebsiteName { get; set; }

        public string RssWebJobName { get; set; }

        public string SpectrumWebJobName { get; set; }

        public string StoreWebJobName { get; set; }

        public string TraceRecipients { get; set; }

        public string AzureFluentClientId { get; set; }

        public string AzureFluentClientSecret { get; set; }

        public string AzureFluentTenantId { get; set; }

        public string AzureFluentSubscriptionId { get; set; }

        public string ScGimpContext { get; set; }

        public string RecaptchaKey { get; set; }

        public ScGimpWebConfig() :
            base()
        {
            Initialize();
        }

        public ScGimpWebConfig(IConfigurationBuilder configBuilder) :
            base(configBuilder)
        {
            Initialize();
        }

        private void Initialize()
        {
            KuduUserName = Configuration["KuduUserName"];
            KuduPassword = Configuration["KuduPassword"];
            AzureWebsiteName = Configuration["AzureWebsiteName"];
            RssWebJobName = Configuration["RssWebJobName"];
            SpectrumWebJobName = Configuration["SpectrumWebJobName"];
            StoreWebJobName = Configuration["StoreWebJobName"];
            TraceRecipients = Configuration["TraceRecipients"];
            AzureFluentClientId = Configuration["AzureFluentClientId"];
            AzureFluentClientSecret = Configuration["AzureFluentClientSecret"];
            AzureFluentTenantId = Configuration["AzureFluentTenantId"];
            AzureFluentSubscriptionId = Configuration["AzureFluentSubscriptionId"];
            ScGimpContext = Configuration["ConnectionStrings:ScGimpContext"];
            RecaptchaKey = Configuration["RecaptchaKey"];
        }
    }
}
