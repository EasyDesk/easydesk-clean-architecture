using EasyDesk.CleanArchitecture.Application.Authorization;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using Microsoft.AspNetCore.Http;
using System.Linq;
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

    public Option<UserInfo> GetUserInfo()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return None;
        }
        return httpContext.User
            .Identities
            .Where(i => i.IsAuthenticated)
            .SelectMany(i => i.FindFirst(ClaimTypes.NameIdentifier).AsOption())
            .Select(c => new UserInfo(c.Value))
            .FirstOption();
    }
}
