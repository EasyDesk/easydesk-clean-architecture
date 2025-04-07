using Autofac;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Host;

public interface ITestHost : IAsyncDisposable
{
    Task Start();

    ILifetimeScope LifetimeScope { get; }
}
