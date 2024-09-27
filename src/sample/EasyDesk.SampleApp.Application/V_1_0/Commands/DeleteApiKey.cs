using EasyDesk.CleanArchitecture.Application.Authentication.ApiKey;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.Commons.Results;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

[AllowUnknownAgent]
public record DeleteApiKey : ICommandRequest<Nothing>, IOverrideMultitenantPolicy
{
    public required string ApiKey { get; init; }

    public MultitenantPolicy GetMultitenantPolicy() => MultitenantPolicies.AnyTenantOrPublic();
}

public class DeleteApiKeyHandler : IHandler<DeleteApiKey>
{
    private readonly IApiKeysStorage _apiKeysStorage;

    public DeleteApiKeyHandler(IApiKeysStorage apiKeysStorage)
    {
        _apiKeysStorage = apiKeysStorage;
    }

    public async Task<Result<Nothing>> Handle(DeleteApiKey request)
    {
        await _apiKeysStorage.DeleteApiKey(request.ApiKey);
        return Ok;
    }
}
