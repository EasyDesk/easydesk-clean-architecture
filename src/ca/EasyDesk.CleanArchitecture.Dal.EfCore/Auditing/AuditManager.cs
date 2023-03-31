using EasyDesk.CleanArchitecture.Application.Auditing;
using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.CleanArchitecture.Dal.EfCore.Auditing.Model;
using EasyDesk.CleanArchitecture.Dal.EfCore.Interfaces.Abstractions;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing;

internal class AuditManager : IAuditStorage, IAuditLog
{
    private readonly AuditContext _context;

    public AuditManager(AuditContext context)
    {
        _context = context;
    }

    public IPageable<AuditRecord> Audit(AuditQuery query)
    {
        IQueryable<AuditRecordModel> result = _context.Records;

        if (query.MatchType)
        {
            result = result.Where(r => r.Type == query.MatchType.Value);
        }

        if (query.MatchName)
        {
            result = result.Where(r => r.Name == query.MatchName.Value);
        }

        if (query.MatchUserId)
        {
            if (query.IsAnonymous.OrElse(false))
            {
                throw new InvalidOperationException("A record can't be anonymous and match given user id at the same time.");
            }
            result = result.Where(r => r.UserId == query.MatchUserId.Value);
        }
        else if (query.IsAnonymous)
        {
            if (query.IsAnonymous.Value)
            {
                result = result.Where(r => r.UserId == null);
            }
            else
            {
                result = result.Where(r => r.UserId != null);
            }
        }

        if (query.IsSuccess)
        {
            result = result.Where(r => r.Success == query.IsSuccess.Value);
        }

        if (query.MatchTimeInterval)
        {
            result = result.Where(r => query.MatchTimeInterval.Value.Contains(r.Instant));
        }

        return result
            .Project<AuditRecordModel, AuditRecord>()
            .ToPageable();
    }

    public Task StoreAudit(AuditRecord record)
    {
        _context.Add(AuditRecordModel.Create(record));
        return _context.SaveChangesAsync();
    }
}
