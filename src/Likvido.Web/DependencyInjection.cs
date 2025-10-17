using Grafana.OpenTelemetry;
using JetBrains.Annotations;
using Likvido.Identity.PrincipalProviders;
using Likvido.Metadata;
using Likvido.Web.PrincipalProviders;
using Likvido.Web.Services.IP;
using Likvido.Web.Services.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;

namespace Likvido.Web;

[PublicAPI]
public static class DependencyInjection
{
    public static IServiceCollection AddLikvidoWeb(this IServiceCollection services, string webAppName)
    {
        var runningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddFilter("Azure", LogLevel.Warning);
            loggingBuilder.AddFilter("Microsoft", LogLevel.Warning);

            loggingBuilder.AddConsole();

            if (runningInContainer)
            {
                loggingBuilder.AddOpenTelemetry(options =>
                {
                    options.UseGrafana(settings =>
                    {
                        settings.ServiceName = webAppName;
                        settings.ResourceAttributes.Add("k8s.pod.name", Environment.GetEnvironmentVariable("HOSTNAME"));
                        settings.ExporterSettings = new AgentOtlpExporter
                        {
                            Protocol = OtlpExportProtocol.Grpc,
                            Endpoint = new Uri("http://grafana-alloy-otlp.grafana-alloy.svc.cluster.local:4317")
                        };
                    });
                    options.IncludeScopes = true;
                });
            }
        });

        services.AddHttpContextAccessor();
        services.AddSingleton(new AppMetadata { AppName = webAppName });
        services.TryAddSingleton<IIpAddressService, IpAddressService>();
        services.TryAddSingleton<IRedirectSecurityService, RedirectSecurityService>();
        services.TryAddSingleton<IPrincipalProvider, WebPrincipalProvider>();

        return services;
    }
}
