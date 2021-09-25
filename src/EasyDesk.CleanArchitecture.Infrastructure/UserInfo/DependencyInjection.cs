using EasyDesk.CleanArchitecture.Application.UserInfo;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Infrastructure.UserInfo
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddTokenInfoRetriever(this IServiceCollection services)
        {
            return services.AddScoped<IUserInfo, TokenInfoRetriever>();
        }
    }
}
