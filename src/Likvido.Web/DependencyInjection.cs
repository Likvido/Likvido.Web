using Grafana.OpenTelemetry;
using JetBrains.Annotations;
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
    public static IServiceCollection AddLikvidoWeb(this IServiceCollection services, string webAppName, string applicationInsightsConnectionString)
    {
        var runningInContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddFilter("Azure", LogLevel.Warning);
            loggingBuilder.AddFilter("Microsoft", LogLevel.Warning);

            loggingBuilder.AddConsole();

            if (runningInContainer)
            {
                if (string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
                {
                    throw new InvalidOperationException("Application Insights configuration is missing. Please ensure the configuration is present in the appsettings.json file when running in a container.");
                }

                loggingBuilder.AddApplicationInsights(
                    telemetryConfiguration =>
                    {
                        telemetryConfiguration.ConnectionString = applicationInsightsConnectionString;
                        telemetryConfiguration.DisableTelemetry = true;
                    },
                    _ => { });

                loggingBuilder.AddOpenTelemetry(options =>
                {
                    options.UseGrafana(settings =>
                    {
                        settings.ServiceName = webAppName;
                        settings.ExporterSettings = new AgentOtlpExporter
                        {
                            Protocol = OtlpExportProtocol.Grpc,
                            Endpoint = new Uri("http://grafana-alloy-otlp.grafana-alloy.svc.cluster.local:4317")
                        };
                    });
                });
            }
        });

        services.AddHttpContextAccessor();
        services.TryAddSingleton<IIpAddressService, IpAddressService>();
        services.TryAddSingleton<IRedirectSecurityService, RedirectSecurityService>();

        return services;
    }
}
