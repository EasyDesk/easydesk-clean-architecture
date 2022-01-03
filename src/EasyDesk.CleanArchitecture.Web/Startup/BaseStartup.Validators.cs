using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.Startup;

public partial class BaseStartup
{
    private void AddRequestValidators(IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining(ApplicationAssemblyMarker);
    }
}
