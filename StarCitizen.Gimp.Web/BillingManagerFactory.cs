using Microsoft.Azure.Management.Billing.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;

namespace StarCitizen.Gimp.Web
{
    public static class BillingManagerFactory
    {
        public static IBillingManager FromDefault()
        {
            ScGimpWebConfig config = new ScGimpWebConfig();

            return FromConfiguration(config);
        }

        public static IBillingManager FromConfiguration(ScGimpWebConfig config)
        {
            return FromServicePrincipal
            (
                config.AzureFluentClientId,
                config.AzureFluentClientSecret,
                config.AzureFluentTenantId,
                config.AzureFluentSubscriptionId
            );
        }

        public static IBillingManager FromServicePrincipal(string clientId, string clientSecret, string tenantId, string subscriptionId)
        {
            AzureCredentialsFactory credentialsFactory = new AzureCredentialsFactory();
            AzureCredentials credentials = credentialsFactory.FromServicePrincipal
            (
                clientId,
                clientSecret,
                tenantId,
                AzureEnvironment.AzureGlobalCloud
            ).WithDefaultSubscription(subscriptionId);
            
            IBillingManager billingManager = BillingManager
                .Configure()
                .Authenticate(credentials, subscriptionId);

            return billingManager;
        }
    }
}
