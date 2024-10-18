using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Messaging;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Application.V_1_0.OutgoingEvents;

namespace EasyDesk.SampleApp.Application.V_1_0.IncomingCommands;

public record GenerateError : IIncomingCommand, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.AnyTenantOrPublic();
}

public class GenerateErrorHandler : IHandler<GenerateError>
{
    public Task<Result<Nothing>> Handle(GenerateError request)
    {
        return Task.FromException<Result<Nothing>>(new Exception("Deliberately throwing an exception"));
    }
}

public record GenerateError2 : IIncomingCommand, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.AnyTenantOrPublic();
}

public class GenerateError2Handler : IHandler<GenerateError2>
{
    public Task<Result<Nothing>> Handle(GenerateError2 request)
    {
        return Task.FromResult(Failure<Nothing>(Errors.Internal(new Exception())));
    }
}

public record GenerateError3 : IIncomingCommand, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.AnyTenantOrPublic();
}

public class GenerateError3Handler : IHandler<GenerateError3>
{
    public Task<Result<Nothing>> Handle(GenerateError3 request)
    {
        throw new NotImplementedException("Deliberately throwing a NotImplementedException");
    }
}

public record GenerateError4 : IIncomingCommand, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.AnyTenantOrPublic();
}

public class GenerateError4Handler : IHandler<GenerateError4>, IFailedMessageHandler<GenerateError4>
{
    private readonly IEventPublisher _eventPublisher;

    public GenerateError4Handler(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    public Task<Result<Nothing>> Handle(GenerateError4 request)
    {
        return Task.FromException<Result<Nothing>>(new Exception("Deliberately throwing an exception"));
    }

    public async Task<Result<Nothing>> HandleFailure(GenerateError4 request)
    {
        await _eventPublisher.Publish(new Error4Handled());
        return Ok;
    }
}
