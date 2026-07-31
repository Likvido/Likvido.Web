using JetBrains.Annotations;
using Likvido.Identity.PrincipalProviders;
using Likvido.Metadata;
using Likvido.Telemetry;
using Likvido.Web.PrincipalProviders;
using Likvido.Web.Services.IP;
using Likvido.Web.Services.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Likvido.Web;

[PublicAPI]
public static class DependencyInjection
{
    public static IServiceCollection AddLikvidoWeb(this IServiceCollection services, string webAppName)
    {
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddFilter("Azure", LogLevel.Warning);
            loggingBuilder.AddFilter("Microsoft", LogLevel.Warning);

            loggingBuilder.AddConsole();

            // Ships logs to the cluster's Grafana Alloy collector, and a no-op anywhere that is not a
            // deployed workload — including a test host booting this application on an in-cluster CI
            // runner. See Likvido.Telemetry for why that case needs saying out loud.
            loggingBuilder.AddLikvidoOtlpLogging(webAppName);
        });

        services.AddHttpContextAccessor();
        services.AddSingleton(new AppMetadata { AppName = webAppName });
        services.TryAddSingleton<IIpAddressService, IpAddressService>();
        services.TryAddSingleton<IRedirectSecurityService, RedirectSecurityService>();
        services.TryAddSingleton<IPrincipalProvider, WebPrincipalProvider>();

        return services;
    }
}
