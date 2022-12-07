using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public record MissingTenantError : Error;

public record MultitenancyNotSupportedError : Error;

public class TenantRequirementStep<T, R> : IPipelineStep<T, R>
{
    private readonly ITenantProvider _tenantProvider;
    private readonly MultitenancyOptions _options;

    public TenantRequirementStep(ITenantProvider tenantProvider, MultitenancyOptions options)
    {
        _tenantProvider = tenantProvider;
        _options = options;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        var policy = typeof(T).GetCustomAttribute<UseMultitenantPolicyAttribute>()
            .AsOption()
            .Map(p => p.Policy)
            .OrElse(_options.DefaultPolicy);

        if (policy == MultitenantPolicy.RequireTenant && !_tenantProvider.IsInTenant())
        {
            return new MissingTenantError();
        }

        if (policy == MultitenantPolicy.RequireNoTenant && _tenantProvider.IsInTenant())
        {
            return new MultitenancyNotSupportedError();
        }

        return await next();
    }
}
