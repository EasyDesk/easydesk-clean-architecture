using Autofac;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Host;

public interface ITestHost : IAsyncDisposable
{
    Task Start();

    ILifetimeScope LifetimeScope { get; }
}
