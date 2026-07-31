using Xunit;

namespace Likvido.Web.Tests;

/// <summary>
/// Applied to every test class that reads or writes process-wide environment variables, so they
/// cannot run concurrently with each other. xunit serialises tests within a single collection, but
/// puts each class in its own collection by default — which would let two such classes race. Naming
/// one collection and sharing it is what actually makes the serialisation hold.
/// </summary>
[CollectionDefinition(Name)]
public class EnvironmentCollection
{
    public const string Name = "Environment variables";
}
