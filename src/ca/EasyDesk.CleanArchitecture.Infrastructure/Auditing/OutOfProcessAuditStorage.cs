using EasyDesk.CleanArchitecture.Application.Auditing;
using System.Threading.Channels;

namespace EasyDesk.CleanArchitecture.Infrastructure.Auditing;

internal class OutOfProcessAuditStorage : IAuditStorage
{
    private readonly ChannelWriter<AuditRecord> _auditsChannel;

    public OutOfProcessAuditStorage(ChannelWriter<AuditRecord> auditsChannel)
    {
        _auditsChannel = auditsChannel;
    }

    public async Task StoreAudit(AuditRecord record)
    {
        await _auditsChannel.WriteAsync(record);
    }
}
