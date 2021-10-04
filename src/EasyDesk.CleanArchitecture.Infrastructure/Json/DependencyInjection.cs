using EasyDesk.CleanArchitecture.Application.Json;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Infrastructure.Json
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddNewtonsoftJsonSerializer(this IServiceCollection services)
        {
            return services.AddSingleton<IJsonSerializer, NewtonsoftJsonSerializer>();
        }
    }
}
