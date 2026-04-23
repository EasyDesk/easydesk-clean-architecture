using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Authentication.ApiKey;

public interface IApiKeysStorage
{
    Task<Option<Agent>> GetAgentForApiKey(string apiKey);

    IAsyncEnumerable<string> GetApiKeys();

    Task StoreApiKey(string apiKey, Agent agent);

    Task DeleteApiKey(string apiKey);

    Task Clear();
}
