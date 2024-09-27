using EasyDesk.CleanArchitecture.Web.Controllers;
using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.SampleApp.Application.V_1_0.Commands;
using EasyDesk.SampleApp.Application.V_1_0.Dto;
using EasyDesk.SampleApp.Application.V_1_0.Queries;
using Microsoft.AspNetCore.Mvc;

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.ApiKeys;

public static class ApiKeysRoutes
{
    private const string Base = "apiKeys";

    public const string StoreApiKey = Base;

    public const string DeleteApiKey = Base + "/{apiKey}";

    public const string TestApiKey = Base;
}

public class ApiKeysController : CleanArchitectureController
{
    [HttpPost(ApiKeysRoutes.StoreApiKey)]
    public async Task<ActionResult<ResponseDto<Nothing, Nothing>>> StoreApiKey(
        [FromBody] StoreApiKey request)
    {
        return await Dispatch(request)
            .ReturnOk();
    }

    [HttpDelete(ApiKeysRoutes.DeleteApiKey)]
    public async Task<ActionResult<ResponseDto<Nothing, Nothing>>> DeleteApiKey(
        [FromRoute] string apiKey)
    {
        var request = new DeleteApiKey
        {
            ApiKey = apiKey,
        };
        return await Dispatch(request)
            .ReturnOk();
    }

    [HttpGet(ApiKeysRoutes.TestApiKey)]
    public async Task<ActionResult<ResponseDto<AgentDto, Nothing>>> TestApiKey()
    {
        var request = new TestApiKey();
        return await Dispatch(request)
            .ReturnOk();
    }
}
