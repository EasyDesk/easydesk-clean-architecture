using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.Authentication.ApiKey;

public record ApiKeyTooLong : ApplicationError
{
    public override string GetDetail() => $"The given Api Key is longer than {ApiKeyValidator.MaxApiKeyLength} characters.";
}

public record InvalidApiKey : ApplicationError
{
    public override string GetDetail() => "The given Api Key was not recognized.";
}

public class ApiKeyValidator
{
    public const int MaxApiKeyLength = 128;

    private readonly IApiKeysStorage _apiKeysStorage;

    public ApiKeyValidator(IApiKeysStorage apiKeysStorage)
    {
        _apiKeysStorage = apiKeysStorage;
    }

    public async Task<Result<Agent>> Authenticate(string apiKey)
    {
        return await ValidateApiKey(apiKey)
            .FlatMapAsync(_ => _apiKeysStorage
                .GetAgentForApiKey(apiKey)
                .ThenOrElseError(() => new InvalidApiKey()));
    }

    private Result<Nothing> ValidateApiKey(string apiKey)
    {
        if (apiKey.Length > MaxApiKeyLength)
        {
            return new ApiKeyTooLong();
        }
        return Ok;
    }
}
