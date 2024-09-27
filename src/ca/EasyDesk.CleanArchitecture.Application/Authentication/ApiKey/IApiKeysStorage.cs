using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Authentication.ApiKey;

public interface IApiKeysStorage
{
    Task<Option<Agent>> GetAgentForApiKey(string apiKey);

    Task StoreApiKey(string apiKey, Agent agent);

    Task DeleteApiKey(string apiKey);
}
