using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using System.Threading.Channels;

namespace EasyDesk.CleanArchitecture.Infrastructure.Auditing;

internal class OutOfProcessAuditStorage : IAuditStorage
{
    private readonly ITenantProvider _tenantProvider;
    private readonly ChannelWriter<(AuditRecord, TenantInfo)> _auditsChannel;

    public OutOfProcessAuditStorage(ITenantProvider tenantProvider, ChannelWriter<(AuditRecord, TenantInfo)> auditsChannel)
    {
        _tenantProvider = tenantProvider;
        _auditsChannel = auditsChannel;
    }

    public async Task StoreAudit(AuditRecord record)
    {
        await _auditsChannel.WriteAsync((record, _tenantProvider.TenantInfo));
    }
}
