using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;

namespace StarCitizen.Gimp.Web
{
    public static class AzureFactory
    {
        public static IAzure FromDefault()
        {
            ScGimpWebConfig config = new ScGimpWebConfig();

            return FromConfiguration(config);
        }

        public static IAzure FromConfiguration(ScGimpWebConfig config)
        {
            return FromServicePrincipal
            (
                config.AzureFluentClientId,
                config.AzureFluentClientSecret,
                config.AzureFluentTenantId,
                config.AzureFluentSubscriptionId
            );
        }

        public static IAzure FromServicePrincipal(string clientId, string clientSecret, string tenantId, string subscriptionId)
        {
            AzureCredentialsFactory credentialsFactory = new AzureCredentialsFactory();
            AzureCredentials credentials = credentialsFactory.FromServicePrincipal
            (
                clientId,
                clientSecret,
                tenantId,
                AzureEnvironment.AzureGlobalCloud
            ).WithDefaultSubscription(subscriptionId);

            IAzure azure = Azure
                .Configure()
                .Authenticate(credentials)
                .WithDefaultSubscription();

            return azure;
        }
    }
}
