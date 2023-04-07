using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Reflection;
using NodaTime;
using System.Collections.Immutable;

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
                Properties: DetectProperties(request),
                UserId: _userInfoProvider.UserInfo.Map(x => x.UserId),
                Success: result.IsSuccess,
                Instant: _clock.GetCurrentInstant()));
    }

    private IImmutableDictionary<string, string> DetectProperties(T request)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, string>();
        if (request is IOverrideAuditProperties propertiesOverride)
        {
            propertiesOverride.ConfigureProperties(builder);
        }
        return EquatableImmutableDictionary<string, string>.FromDictionary(builder.ToImmutable());
    }

    private Option<string> DetectDescription(T request)
    {
        if (request is IOverrideAuditDescription descriptionOverride)
        {
            return Some(descriptionOverride.GetAuditDescription());
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
