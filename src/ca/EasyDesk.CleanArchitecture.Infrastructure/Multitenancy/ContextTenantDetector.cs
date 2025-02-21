using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Infrastructure.Context;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;

public class ContextTenantDetector : IContextTenantDetector
{
    private readonly ContextDetector _contextDetector;
    private readonly IAgentProvider _agentProvider;
    private readonly HttpRequestTenantReader _httpRequestTenantReader;
    private readonly AsyncMessageTenantReader _asyncMessageTenantReader;

    public ContextTenantDetector(
        ContextDetector contextDetector,
        IAgentProvider agentProvider,
        HttpRequestTenantReader httpRequestTenantReader,
        AsyncMessageTenantReader asyncMessageTenantReader)
    {
        _contextDetector = contextDetector;
        _agentProvider = agentProvider;
        _httpRequestTenantReader = httpRequestTenantReader;
        _asyncMessageTenantReader = asyncMessageTenantReader;
    }

    public Option<string> TenantId => _contextDetector.MatchContext(
        httpContext: c => _httpRequestTenantReader(c, _agentProvider.Agent),
        messageContext: c => _asyncMessageTenantReader(c),
        other: () => None);
}
