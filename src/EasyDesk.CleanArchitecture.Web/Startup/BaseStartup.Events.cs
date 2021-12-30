using EasyDesk.CleanArchitecture.Application.Events.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Startup
{
    public partial class BaseStartup
    {
        protected abstract bool UsesPublisher { get; }

        protected abstract bool UsesConsumer { get; }

        protected abstract IEventBusImplementation EventBusImplementation { get; }

        private void AddEventManagement(IServiceCollection services)
        {
            if (UsesConsumer || UsesPublisher)
            {
                var builder = services.AddEventManagement(EventBusImplementation, IsMultitenant);
                if (UsesPublisher)
                {
                    builder.AddOutboxPublisher();
                }
                if (UsesConsumer)
                {
                    builder.AddIdempotentConsumer(ApplicationAssemblyMarker);
                }
            }
        }
    }
}
