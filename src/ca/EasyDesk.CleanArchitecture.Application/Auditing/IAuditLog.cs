using EasyDesk.CleanArchitecture.Application.Pagination;

namespace EasyDesk.CleanArchitecture.Application.Auditing;

public interface IAuditLog
{
    IPageable<AuditRecord> Audit(AuditQuery query);
}
