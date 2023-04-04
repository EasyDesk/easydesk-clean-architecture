using EasyDesk.CleanArchitecture.Application.Auditing;

namespace EasyDesk.CleanArchitecture.Infrastructure.Auditing;

public interface IAuditStorageImplementation
{
    Task StoreAudit(AuditRecord record);
}
