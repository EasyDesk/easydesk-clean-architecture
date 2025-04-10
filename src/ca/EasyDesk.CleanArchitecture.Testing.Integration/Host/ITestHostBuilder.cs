using Autofac;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Host;

public interface ITestHostBuilder
{
    ITestHostBuilder WithEnvironment(string environment);

    ITestHostBuilder ConfigureContainer(Action<ContainerBuilder> configure);

    ITestHostBuilder WithConfiguration(string key, string value);
}
