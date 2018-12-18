using Microsoft.Extensions.Configuration;
using StarCitizen.Gimp.Core;

namespace StarCitizen.Gimp.Web
{
    public class ScGimpWebConfig : ScGimpCoreConfig
    {
        public string TraceRecipients { get; set; }

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
            TraceRecipients = Configuration["TraceRecipients"];
            ScGimpContext = Configuration["ConnectionStrings:ScGimpContext"];
            RecaptchaKey = Configuration["RecaptchaKey"];
        }
    }
}
