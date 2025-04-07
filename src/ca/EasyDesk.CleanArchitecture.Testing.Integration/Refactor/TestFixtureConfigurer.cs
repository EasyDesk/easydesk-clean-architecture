using Autofac;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor;

public class TestFixtureConfigurer
{
    public ContainerBuilder ContainerBuilder { get; } = new();

    public IContainer BuildContainer() => ContainerBuilder.Build();
}
