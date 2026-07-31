using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace Likvido.Web.Tests;

// Both tests mutate process-wide environment variables. Keeping them in a single class puts them in
// one xunit collection, so they run sequentially and cannot race each other.
public class DependencyInjectionTests
{
    // AddLikvidoWeb only wires up OpenTelemetry when it thinks it is running in a container, so the
    // tests have to pretend they are - otherwise the interesting code path is skipped entirely.
    private const string RunningInContainerVariable = "DOTNET_RUNNING_IN_CONTAINER";
    private const string HostnameVariable = "HOSTNAME";

    [Fact]
    public void AddLikvidoWeb_WithoutHostname_ResolvesLoggerFactory()
    {
        // BuildKit does not set HOSTNAME inside a RUN step, so any test booting the application from
        // a Docker build stage used to fail here with
        // "Attribute value type is not an accepted primitive (Parameter 'k8s.pod.name')".
        RunWithEnvironment(hostname: null, () =>
        {
            var provider = new ServiceCollection().AddLikvidoWeb("test-app").BuildServiceProvider();

            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("test");

            logger.ShouldNotBeNull();
        });
    }

    [Fact]
    public void AddLikvidoWeb_WithHostname_ResolvesLoggerFactory()
    {
        RunWithEnvironment(hostname: "test-app-6c9f8d7b5c-2xq4t", () =>
        {
            var provider = new ServiceCollection().AddLikvidoWeb("test-app").BuildServiceProvider();

            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("test");

            logger.ShouldNotBeNull();
        });
    }

    // The service provider is deliberately not disposed: shutting down the OTLP exporter waits on a
    // flush timeout against an endpoint that does not exist outside the cluster, which adds seconds
    // of dead wall clock to every test.
    private static void RunWithEnvironment(string? hostname, Action action)
    {
        var originalRunningInContainer = Environment.GetEnvironmentVariable(RunningInContainerVariable);
        var originalHostname = Environment.GetEnvironmentVariable(HostnameVariable);

        try
        {
            Environment.SetEnvironmentVariable(RunningInContainerVariable, "true");
            Environment.SetEnvironmentVariable(HostnameVariable, hostname);

            action();
        }
        finally
        {
            Environment.SetEnvironmentVariable(RunningInContainerVariable, originalRunningInContainer);
            Environment.SetEnvironmentVariable(HostnameVariable, originalHostname);
        }
    }
}
