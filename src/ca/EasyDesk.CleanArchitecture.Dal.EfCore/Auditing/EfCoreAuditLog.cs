using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using Microsoft.EntityFrameworkCore;

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
        if (query.MatchIdentity.IsPresent && query.IsAnonymous.Contains(true))
        {
            throw new InvalidOperationException("A record can't be anonymous and match given identity id at the same time.");
        }

        return _context.AuditRecords
            .AsNoTracking()
            .AsSplitQuery()
            .Conditionally(query.FromInstant, from => q => q.Where(r => r.Instant >= from))
            .Conditionally(query.ToInstant, to => q => q.Where(r => r.Instant <= to))
            .Conditionally(query.MatchType, type => q => q.Where(r => r.Type == type))
            .Conditionally(query.MatchName, name => q => q.Where(r => r.Name == name))
            .Conditionally(query.IsSuccess, success => q => q.Where(r => r.Success == success))
            .Conditionally(query.MatchIdentity, identity => q => q.Where(r => r.Identity == identity))
            .Conditionally(query.IsAnonymous, anonymous => q => q.Where(anonymous
                ? r => r.Identity == null
                : r => r.Identity != null))
            .OrderBy(r => r.Instant)
            .ThenBy(r => r.Id)
            .Select(src => src.ToAuditRecord())
            .ToPageable();
    }
}
