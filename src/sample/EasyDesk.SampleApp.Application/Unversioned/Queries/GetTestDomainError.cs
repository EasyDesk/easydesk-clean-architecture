using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.Commons.Results;
using EasyDesk.SampleApp.Domain.DomainErrors;

namespace EasyDesk.SampleApp.Application.Unversioned.Queries;

[AllowUnknownAgent]
public record GetTestDomainError : IQueryRequest<Nothing>, IOverrideMultitenantPolicy
{
    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.AnyTenantOrPublic();
}

public class GetTestDomainErrorHandler : IHandler<GetTestDomainError>
{
    public Task<Result<Nothing>> Handle(GetTestDomainError request)
    {
        return Task.FromResult(Failure<Nothing>(new TestDomainError("Test domain error content")));
    }
}
