using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public sealed class MultitenancyManagementStep<T, R> : IPipelineStep<T, R>
    where R : notnull
{
    private readonly IContextTenantReader _contextTenantReader;
    private readonly IContextTenantInitializer _tenantInitializer;
    private readonly IMultitenancyManager _multitenancyManager;
    private readonly MultitenantPolicy _defaultPolicy;

    public MultitenancyManagementStep(
        IContextTenantReader contextTenantReader,
        IContextTenantInitializer tenantInitializer,
        IMultitenancyManager multitenancyManager,
        MultitenantPolicy defaultPolicy)
    {
        _contextTenantReader = contextTenantReader;
        _tenantInitializer = tenantInitializer;
        _multitenancyManager = multitenancyManager;
        _defaultPolicy = defaultPolicy;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        var policy = GetPolicyForRequest(request);

        var rawTenantId = _contextTenantReader.GetTenantId();

        return await ValidateTenantId(rawTenantId)
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
