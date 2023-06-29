using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;
using EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public sealed class MultitenancyOptions
{
    public HttpRequestTenantReader HttpRequestTenantReader { get; private set; } =
        (c, _) => CommonTenantReaders.ReadFromHttpContext(c);

    public AsyncMessageTenantReader AsyncMessageTenantReader { get; private set; } =
        CommonTenantReaders.ReadFromMessageContext;

    public MultitenantPolicy DefaultPolicy { get; private set; } =
        MultitenantPolicies.IgnoreAndUsePublic();

    public MultitenancyOptions WithDefaultPolicy(MultitenantPolicy policy)
    {
        DefaultPolicy = policy;
        return this;
    }

    public MultitenancyOptions WithHttpRequestTenantReader(HttpRequestTenantReader reader)
    {
        HttpRequestTenantReader = reader;
        return this;
    }

    public MultitenancyOptions WithAsyncMessageTenantReader(AsyncMessageTenantReader reader)
    {
        AsyncMessageTenantReader = reader;
        return this;
    }

    public MultitenancyOptions ReadFromAgent(Func<Agent, Option<string>> reader) =>
        WithHttpRequestTenantReader((_, agent) => agent.FlatMap(reader));

    public MultitenancyOptions ReadFromAttribute(Realm realm, string attributeKey) =>
        ReadFromAgent(agent => agent.GetIdentity(realm).FlatMap(i => i.Attributes.GetSingle(attributeKey)));
}
