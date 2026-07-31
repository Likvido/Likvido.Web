using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace Likvido.Web.Tests;

// These tests mutate process-wide environment variables. Keeping them in a single class puts them in
// one xunit collection, so they run sequentially and cannot race each other.
public class DependencyInjectionTests
{
    // Set by kubelet in every pod, and what Likvido.Telemetry reads to decide whether to export. The
    // tests set it to exercise both sides of that decision through AddLikvidoWeb.
    private const string KubernetesServiceHostVariable = "KUBERNETES_SERVICE_HOST";

    // ⚠️ Cleared for the duration of every test. These tests run on a GitHub-hosted runner, where
    // GITHUB_ACTIONS is "true" and Likvido.Telemetry therefore declines to export — which would make
    // the in-a-pod test below quietly assert nothing in CI while still passing locally.
    private const string GitHubActionsVariable = "GITHUB_ACTIONS";

    [Fact]
    public void AddLikvidoWeb_OutsideACluster_ResolvesLoggerFactory()
    {
        RunWithEnvironment(kubernetesServiceHost: null, () =>
        {
            var provider = new ServiceCollection().AddLikvidoWeb("test-app").BuildServiceProvider();

            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("test");

            logger.ShouldNotBeNull();
        });
    }

    [Fact]
    public void AddLikvidoWeb_InAPod_ResolvesLoggerFactory()
    {
        // The path that wires up the OTLP exporter. What the exporter itself does with a missing
        // HOSTNAME, and when it declines to export at all, belongs to Likvido.Telemetry and is
        // covered by its own tests — this only proves AddLikvidoWeb composes with it.
        RunWithEnvironment(kubernetesServiceHost: "172.19.0.1", () =>
        {
            var provider = new ServiceCollection().AddLikvidoWeb("test-app").BuildServiceProvider();

            var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("test");

            logger.ShouldNotBeNull();
        });
    }

    // The service provider is deliberately not disposed: shutting down the OTLP exporter waits on a
    // flush timeout against an endpoint that does not exist outside the cluster, which adds seconds
    // of dead wall clock to every test.
    private static void RunWithEnvironment(string? kubernetesServiceHost, Action action)
    {
        var originalServiceHost = Environment.GetEnvironmentVariable(KubernetesServiceHostVariable);
        var originalGitHubActions = Environment.GetEnvironmentVariable(GitHubActionsVariable);

        try
        {
            Environment.SetEnvironmentVariable(KubernetesServiceHostVariable, kubernetesServiceHost);
            Environment.SetEnvironmentVariable(GitHubActionsVariable, null);

            action();
        }
        finally
        {
            Environment.SetEnvironmentVariable(KubernetesServiceHostVariable, originalServiceHost);
            Environment.SetEnvironmentVariable(GitHubActionsVariable, originalGitHubActions);
        }
    }
}
