using Autofac;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor;

public interface ITestHost
{
    ILifetimeScope LifetimeScope { get; }
}
