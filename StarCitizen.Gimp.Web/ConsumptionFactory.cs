using Microsoft.Azure.Management.Consumption.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;

namespace StarCitizen.Gimp.Web
{
    public static class ConsumptionFactory
    {
        public static IConsumptionManager FromDefault()
        {
            ScGimpWebConfig config = new ScGimpWebConfig();

            return FromConfiguration(config);
        }

        public static IConsumptionManager FromConfiguration(ScGimpWebConfig config)
        {
            return FromServicePrincipal
            (
                config.AzureFluentClientId,
                config.AzureFluentClientSecret,
                config.AzureFluentTenantId,
                config.AzureFluentSubscriptionId
            );
        }

        public static IConsumptionManager FromServicePrincipal(string clientId, string clientSecret, string tenantId, string subscriptionId)
        {
            AzureCredentialsFactory credentialsFactory = new AzureCredentialsFactory();
            AzureCredentials credentials = credentialsFactory.FromServicePrincipal
            (
                clientId,
                clientSecret,
                tenantId,
                AzureEnvironment.AzureGlobalCloud
            ).WithDefaultSubscription(subscriptionId);

            IConsumptionManager consumptionManager = ConsumptionManager
                .Configure()
                .Authenticate(credentials, subscriptionId);

            return consumptionManager;
        }
    }
}
