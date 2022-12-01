using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using System.Reflection;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public class TenantRequirementStep<T, R> : IPipelineStep<T, R>
{
    private readonly ITenantProvider _tenantProvider;

    public TenantRequirementStep(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        if (typeof(T).GetCustomAttribute<AllowNoTenantAttribute>() is not null)
        {
            return await next();
        }
        if (!_tenantProvider.IsInTenant())
        {
            return new MissingTenantError();
        }
        return await next();
    }
}
