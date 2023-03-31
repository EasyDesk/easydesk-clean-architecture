using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Reflection;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Application.Auditing;

public class AuditingStep<T, R> : IPipelineStep<T, R>
    where R : notnull
    where T : IReadWriteOperation
{
    private readonly IAuditStorage _auditStorage;
    private readonly IUserInfoProvider _userInfoProvider;
    private readonly IClock _clock;

    public AuditingStep(
        IAuditStorage auditStorage,
        IUserInfoProvider userInfoProvider,
        IClock clock)
    {
        _auditStorage = auditStorage;
        _userInfoProvider = userInfoProvider;
        _clock = clock;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        var result = await next();

        await ComputeAuditRecord(request, result)
            .IfPresentAsync(_auditStorage.StoreAudit);

        return result;
    }

    private Option<AuditRecord> ComputeAuditRecord(T request, Result<R> result)
    {
        return DetectAuditRecordType()
            .Map(type => new AuditRecord(
                Type: type,
                Name: typeof(T).Name,
                Description: DetectDescription(request),
                UserId: _userInfoProvider.UserInfo.Map(x => x.UserId),
                Success: result.IsSuccess,
                Instant: _clock.GetCurrentInstant()));
    }

    private Option<string> DetectDescription(T request)
    {
        if (request is IOverrideAuditDescription descriptionProvider)
        {
            return Some(descriptionProvider.GetAuditDescription());
        }
        return None;
    }

    private Option<AuditRecordType> DetectAuditRecordType()
    {
        if (typeof(T).IsSubtypeOrImplementationOf(typeof(ICommandRequest<>)))
        {
            return Some(AuditRecordType.CommandRequest);
        }
        else if (typeof(T).IsSubtypeOrImplementationOf(typeof(IEvent)))
        {
            return Some(AuditRecordType.Event);
        }
        else if (typeof(T).IsSubtypeOrImplementationOf(typeof(ICommand)))
        {
            return Some(AuditRecordType.Command);
        }
        return None;
    }
}
