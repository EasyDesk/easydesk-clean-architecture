using EasyDesk.CleanArchitecture.Application.Authentication.ApiKey;
using EasyDesk.CleanArchitecture.Application.Authorization.Static;
using EasyDesk.CleanArchitecture.Application.Cqrs.Sync;
using EasyDesk.CleanArchitecture.Application.Dispatching;
using EasyDesk.Commons.Results;

namespace EasyDesk.SampleApp.Application.V_1_0.Commands;

[AllowUnknownAgent]
public record DeleteApiKey : ICommandRequest<Nothing>
{
    public required string ApiKey { get; init; }
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
