using EasyDesk.CleanArchitecture.Application.UserInfo;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Web.UserInfo
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUserInfo(this IServiceCollection services)
        {
            return services.AddScoped<IUserInfo, TokenInfoRetriever>();
        }
    }
}
