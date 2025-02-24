using Autofac;
using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Infrastructure.Seeding;

public static class DispatcherFactoryExtensions
{
    public static IDispatcher CreateSeedingDispatcher(this DispatcherFactory factory, Agent? agent = null, string? tenantId = null) =>
        factory.CreateDispatcherWithCustomServices(builder =>
        {
            builder.RegisterInstance(new SeedingContextTenantDetector(tenantId.AsOption())).As<IContextTenantDetector>().SingleInstance();

            builder.RegisterInstance(new SeedingAuthenticationService(agent.AsOption())).As<IAuthenticationService>().SingleInstance();
        });

    private class SeedingContextTenantDetector : IContextTenantDetector
    {
        public SeedingContextTenantDetector(Option<string> tenantId)
        {
            TenantId = tenantId;
        }

        public Option<string> TenantId { get; }
    }

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
