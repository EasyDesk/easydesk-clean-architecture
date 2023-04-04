using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Dal.EfCore.Auditing.Model;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing;

internal class EfCoreAuditStorage : IAuditStorage
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EfCoreAuditStorage(ITenantProvider tenantProvider, IServiceScopeFactory serviceScopeFactory)
    {
        _tenantProvider = tenantProvider;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task StoreAudit(AuditRecord record)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<IContextTenantInitializer>().Initialize(_tenantProvider.TenantInfo);
        var context = scope.ServiceProvider.GetRequiredService<AuditingContext>();
        context.Add(AuditRecordModel.Create(record));
        await context.SaveChangesAsync();
    }
}
