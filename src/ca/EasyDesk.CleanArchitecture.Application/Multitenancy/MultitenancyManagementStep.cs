using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public sealed class MultitenancyManagementStep<T, R> : IPipelineStep<T, R>
{
    private readonly IContextProvider _contextProvider;
    private readonly IContextTenantInitializer _tenantInitializer;
    private readonly IMultitenancyManager _multitenancyManager;
    private readonly MultitenantPolicy _defaultPolicy;

    public MultitenancyManagementStep(
        IContextProvider contextProvider,
        IContextTenantInitializer tenantInitializer,
        IMultitenancyManager multitenancyManager,
        MultitenantPolicy defaultPolicy)
    {
        _contextProvider = contextProvider;
        _tenantInitializer = tenantInitializer;
        _multitenancyManager = multitenancyManager;
        _defaultPolicy = defaultPolicy;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        var policy = GetPolicyForRequest(request);

        return await ValidateTenantId(_contextProvider.TenantId)
            .FlatMapAsync(t => policy(t, _multitenancyManager))
            .ThenIfSuccess(_tenantInitializer.Initialize)
            .ThenFlatMapAsync(_ => next());
    }

    private Result<TenantInfo> ValidateTenantId(Option<string> rawTenantId) => rawTenantId.Match(
        some: t => TenantId.TryNew(t).OrElseError(() => new InvalidTenantIdError(t)).Map(TenantInfo.Tenant),
        none: () => Success(TenantInfo.Public));

    private MultitenantPolicy GetPolicyForRequest(T request) => request switch
    {
        IOverrideMultitenantPolicy p => p.GetMultitenantPolicy(),
        _ => _defaultPolicy
    };
}
