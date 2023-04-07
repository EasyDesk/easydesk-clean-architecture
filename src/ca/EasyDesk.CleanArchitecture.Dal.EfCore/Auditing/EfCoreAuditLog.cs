using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Dal.EfCore.Auditing.Model;
using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing;

internal class EfCoreAuditLog : IAuditLog
{
    private readonly AuditingContext _context;

    public EfCoreAuditLog(AuditingContext context)
    {
        _context = context;
    }

    public IPageable<AuditRecord> Audit(AuditQuery query)
    {
        if (query.MatchUserId.IsPresent && query.IsAnonymous.Contains(true))
        {
            throw new InvalidOperationException("A record can't be anonymous and match given user id at the same time.");
        }

        return _context.AuditRecords
            .Conditionally(
                query.MatchTimeInterval.HasStart || query.MatchTimeInterval.HasEnd,
                q => q.Where(r => query.MatchTimeInterval.Contains(r.Instant)))
            .Conditionally(query.MatchType, type => q => q.Where(r => r.Type == type))
            .Conditionally(query.MatchName, name => q => q.Where(r => r.Name == name))
            .Conditionally(query.IsSuccess, success => q => q.Where(r => r.Success == success))
            .Conditionally(query.MatchUserId, userId => q => q.Where(r => r.UserId == userId))
            .Conditionally(query.IsAnonymous, anonymous => q => q.Where(anonymous
                ? r => r.UserId == null
                : r => r.UserId != null))
            .Project<AuditRecordModel, AuditRecord>()
            .ToPageable();
    }
}
