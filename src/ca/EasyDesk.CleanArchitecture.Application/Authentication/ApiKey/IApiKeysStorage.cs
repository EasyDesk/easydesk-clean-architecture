using EasyDesk.CleanArchitecture.Application.Pagination;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Authentication.ApiKey;

public interface IApiKeysStorage
{
    Task<Option<Agent>> GetAgentForApiKey(string apiKey);

    IPageable<string> GetApiKeys();

    Task StoreApiKey(string apiKey, Agent agent);

    Task DeleteApiKey(string apiKey);

    Task Clear();
}
