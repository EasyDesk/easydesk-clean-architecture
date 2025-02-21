using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Authentication.ApiKey;
using EasyDesk.CleanArchitecture.Dal.EfCore.Auth.Model;
using EasyDesk.CleanArchitecture.Dal.EfCore.Utils;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auth;

internal class EfCoreApiKeysStorage : IApiKeysStorage
{
    private readonly AuthContext _authContext;

    public EfCoreApiKeysStorage(AuthContext authContext)
    {
        _authContext = authContext;
    }

    public async Task StoreApiKey(string apiKey, Agent agent)
    {
        var existing = await FindApiKey(apiKey);

        existing.Match(
            some: a =>
            {
                a.UpdateIdentities(agent);
                _authContext.ApiKeys.Update(a);
            },
            none: () =>
            {
                var model = CreateNewApiKey(apiKey, agent);
                _authContext.ApiKeys.Add(model);
            });

        await _authContext.SaveChangesAsync();
    }

    private static ApiKeyModel CreateNewApiKey(string apiKey, Agent agent) =>
        new ApiKeyModel { ApiKey = apiKey }.Also(x => x.UpdateIdentities(agent));

    public async Task DeleteApiKey(string apiKey)
    {
        var model = await FindApiKey(apiKey);

        model.IfPresent(x => _authContext.ApiKeys.Remove(x));

        await _authContext.SaveChangesAsync();
    }

    private async Task<Option<ApiKeyModel>> FindApiKey(string apiKey)
    {
        return await _authContext.ApiKeys
            .Where(x => x.ApiKey == apiKey)
            .FirstOptionAsync();
    }

    public async Task<Option<Agent>> GetAgentForApiKey(string apiKey)
    {
        return await FindApiKey(apiKey)
            .ThenMap(x => x.GetAgent());
    }
}
