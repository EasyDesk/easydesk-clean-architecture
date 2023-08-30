using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.Commons.Results;

namespace EasyDesk.SampleApp.Application.V_1_0.Queries;

public record GetAudits(AuditQuery Query) : IQueryRequest<IPageable<AuditRecordDto>>;

public class GetAuditsHandler : IHandler<GetAudits, IPageable<AuditRecordDto>>
{
    private readonly IAuditLog _auditLog;

    public GetAuditsHandler(IAuditLog auditLog)
    {
        _auditLog = auditLog;
    }

    public Task<Result<IPageable<AuditRecordDto>>> Handle(GetAudits request) =>
        Task.FromResult(Success(_auditLog.Audit(request.Query).Map(AuditRecordDto.MapFrom)));
}
