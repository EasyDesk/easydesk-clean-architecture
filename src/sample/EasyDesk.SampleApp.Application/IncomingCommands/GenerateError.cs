using EasyDesk.CleanArchitecture.Application.Cqrs.Async;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;

namespace EasyDesk.SampleApp.Application.IncomingCommands;

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
