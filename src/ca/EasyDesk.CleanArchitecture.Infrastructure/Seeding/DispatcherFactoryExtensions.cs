using Autofac;
using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Infrastructure.Seeding;

public static class DispatcherFactoryExtensions
{
    public static IDispatcher CreateProgrammaticDispatcher(this DispatcherFactory factory, Agent? agent = null, Action<ContainerBuilder>? setupScope = null) =>
        factory.CreateDispatcherWithCustomServices(builder =>
        {
            builder.RegisterInstance(new SeedingAuthenticationService(agent.AsOption())).As<IAuthenticationService>().SingleInstance();

            setupScope?.Invoke(builder);
        });

    private class SeedingAuthenticationService : IAuthenticationService
    {
        private readonly Option<Agent> _agent;

        public SeedingAuthenticationService(Option<Agent> agent)
        {
            _agent = agent;
        }

        public Task<Option<(AuthenticationResult Result, string Scheme)>> Authenticate()
        {
            return Task.FromResult(_agent
                .Map(a => new AuthenticationResult.Authenticated
                {
                    Agent = a,
                })
                .Map(r => (r as AuthenticationResult, "seeding")));
        }
    }
}
