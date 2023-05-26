using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Multitenancy;

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
        return Task.FromResult(Failure<Nothing>(Errors.Generic("Deliberately returning error")));
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
