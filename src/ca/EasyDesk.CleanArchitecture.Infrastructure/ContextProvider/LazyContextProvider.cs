using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

internal sealed class LazyContextProvider : IContextProvider
{
    private readonly Lazy<ContextInfo> _contextInfo;
    private readonly Lazy<Option<string>> _tenantId;
    private readonly Lazy<CancellationToken> _cancellationToken;

    public LazyContextProvider(IContextProvider innerContextProvider)
    {
        _contextInfo = new(() => innerContextProvider.CurrentContext);
        _tenantId = new(() => innerContextProvider.TenantId);
        _cancellationToken = new(() => innerContextProvider.CancellationToken);
    }

    public ContextInfo CurrentContext => _contextInfo.Value;

    public Option<string> TenantId => _tenantId.Value;

    public CancellationToken CancellationToken => _cancellationToken.Value;
}
