using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Pagination;

namespace EasyDesk.SampleApp.Application.Queries;

public record GetAudits(AuditQuery Query) : IQueryRequest<IPageable<AuditRecord>>;

public class GetAuditsHandler : IHandler<GetAudits, IPageable<AuditRecord>>
{
    private readonly IAuditLog _auditLog;

    public GetAuditsHandler(IAuditLog auditLog)
    {
        _auditLog = auditLog;
    }

    public Task<Result<IPageable<AuditRecord>>> Handle(GetAudits request) =>
        Task.FromResult(Success(_auditLog.Audit(request.Query)));
}
