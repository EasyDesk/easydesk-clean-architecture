using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.CleanArchitecture.Infrastructure.Jwt;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Web.Authentication;

public class HttpContextUserInfoProvider : IUserInfoProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextUserInfoProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Option<UserInfo> GetUserInfo() =>
        _httpContextAccessor.HttpContext switch
        {
            { User: var user } when user.Identity.IsAuthenticated => new UserInfo(user.FindFirstValue(JwtClaimNames.Subject)),
            _ => None
        };
}
