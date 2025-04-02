using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.Commons.Observables;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor;

public class TestFixtureConfigurer
{
    public required IAsyncObservable<Nothing> OnInitialization { get; init; }

    public required IAsyncObservable<Nothing> BeforeEachTest { get; init; }

    public required IAsyncObservable<Nothing> AfterEachTest { get; init; }

    public required IAsyncObservable<Nothing> OnReset { get; init; }

    public required IAsyncObservable<Nothing> OnDisposal { get; init; }

    public required ITestHostBuilder HostBuilder { get; init; }

    public required ContainersCollection Containers { get; init; }
}
