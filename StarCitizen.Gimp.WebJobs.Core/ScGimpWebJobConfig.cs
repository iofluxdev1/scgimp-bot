using Microsoft.Extensions.Configuration;
using StarCitizen.Gimp.Core;
using System;

namespace StarCitizen.Gimp.WebJobs.Core
{
    public class ScGimpWebJobConfig : ScGimpCoreConfig
    {
        public string TraceRecipients { get; set; }

        public bool Debug { get; set; }

        public ScGimpWebJobConfig() :
            base()
        {
            Initialize();
        }

        public ScGimpWebJobConfig(IConfigurationBuilder configBuilder) :
            base(configBuilder)
        {
            Initialize();
        }

        private void Initialize()
        {
            Debug = false;

            if (Boolean.TryParse(Configuration["Debug"], out bool debug))
            {
                Debug = debug;
            }

            TraceRecipients = Configuration["TraceRecipients"];
        }
    }
}
