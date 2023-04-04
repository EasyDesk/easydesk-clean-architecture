using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Dal.EfCore.Auditing.Model;
using EasyDesk.CleanArchitecture.Infrastructure.Auditing;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing;

internal class EfCoreAuditStorage : IAuditStorageImplementation
{
    private readonly AuditingContext _context;

    public EfCoreAuditStorage(AuditingContext context)
    {
        _context = context;
    }

    public async Task StoreAudit(AuditRecord record)
    {
        _context.Add(AuditRecordModel.Create(record));
        await _context.SaveChangesAsync();
    }
}
