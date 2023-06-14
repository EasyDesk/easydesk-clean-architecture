using EasyDesk.CleanArchitecture.Application.Authorization.Model;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public class CachedAuthorizationProvider : IAuthorizationProvider
{
    private readonly IAuthorizationProvider _innerProvider;
    private Option<Option<AuthorizationInfo>> _cache;

    public CachedAuthorizationProvider(IAuthorizationProvider innerProvider)
    {
        _innerProvider = innerProvider;
    }

    public Task<Option<AuthorizationInfo>> GetAuthorizationInfo() =>
        _cache.OrElseGetAsync(async () =>
        {
            var authInfo = await _innerProvider.GetAuthorizationInfo();
            _cache = Some(authInfo);
            return authInfo;
        });
}
