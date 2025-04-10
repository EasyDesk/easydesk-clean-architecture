using Autofac;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Session;

public class SessionConfigurer
{
    public required ContainerBuilder ContainerBuilder { get; init; }
}
