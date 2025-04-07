using Autofac;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Session;

public class SessionConfigurer
{
    public required ContainerBuilder ContainerBuilder { get; init; }
}
