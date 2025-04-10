using Autofac;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Fixture;

public class TestFixtureConfigurer
{
    public ContainerBuilder ContainerBuilder { get; } = new();

    public IContainer BuildContainer() => ContainerBuilder.Build();
}
