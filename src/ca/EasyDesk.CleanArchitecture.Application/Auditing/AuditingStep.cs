using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Reflection;
using EasyDesk.Commons.Results;
using NodaTime;

namespace EasyDesk.CleanArchitecture.Application.Auditing;

public sealed class AuditingStep<T, R> : IPipelineStep<T, R>
    where T : IReadWriteOperation
{
    private readonly IAuditStorage _auditStorage;
    private readonly IAgentProvider _agentProvider;
    private readonly IAuditConfigurer _auditConfigurer;
    private readonly IClock _clock;

    public AuditingStep(
        IAuditStorage auditStorage,
        IAgentProvider agentProvider,
        IAuditConfigurer auditConfigurer,
        IClock clock)
    {
        _auditStorage = auditStorage;
        _agentProvider = agentProvider;
        _auditConfigurer = auditConfigurer;
        _clock = clock;
    }

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        var result = await next();

        await ComputeAuditRecord(result)
            .IfPresentAsync(_auditStorage.StoreAudit);

        return result;
    }

    private Option<AuditRecord> ComputeAuditRecord(Result<R> result)
    {
        return DetectAuditRecordType()
            .Map(type => new AuditRecord(
                Type: type,
                Name: typeof(T).Name,
                Description: _auditConfigurer.Description,
                Agent: _agentProvider.Agent,
                Properties: _auditConfigurer.Properties,
                Success: result.IsSuccess,
                Instant: _clock.GetCurrentInstant()));
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
