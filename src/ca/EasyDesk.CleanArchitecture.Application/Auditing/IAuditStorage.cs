namespace EasyDesk.CleanArchitecture.Application.Auditing;

public interface IAuditStorage
{
    Task StoreAudit(AuditRecord record);
}
