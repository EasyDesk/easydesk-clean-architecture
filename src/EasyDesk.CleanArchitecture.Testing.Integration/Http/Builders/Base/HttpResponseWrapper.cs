using EasyDesk.CleanArchitecture.Web.Dto;
using Newtonsoft.Json;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public class HttpResponseWrapper<T, M> : ResponseCache<HttpResponseMessage>
{
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public HttpResponseWrapper(HttpResponseMessage httpResponseMessage, JsonSerializerSettings jsonSerializerSettings)
        : this(() => Task.FromResult(httpResponseMessage), jsonSerializerSettings)
    {
    }

    public HttpResponseWrapper(AsyncFunc<HttpResponseMessage> httpResponseMessage, JsonSerializerSettings jsonSerializerSettings)
        : base(httpResponseMessage)
    {
        _jsonSerializerSettings = jsonSerializerSettings;
    }

    public Task<bool> IsSuccess => Response.Map(r => r.IsSuccessStatusCode && r.Content is not null);

    public async Task EnsureSuccess()
    {
        if (!await IsSuccess)
        {
            throw await HttpRequestUnexpectedFailureException.Create(await Response);
        }
    }

    private async Task<ResponseDto<T, M>> ParseContent()
    {
        var bodyAsJson = await (await Response).Content.ReadAsStringAsync();
        try
        {
            return JsonConvert.DeserializeObject<ResponseDto<T, M>>(bodyAsJson, _jsonSerializerSettings);
        }
        catch (JsonException e)
        {
            throw new Exception($"Failed to parse response as {typeof(T).Name}. Content was:\n\n{bodyAsJson}", e);
        }
    }

    public Task<VerifiableHttpResponse<T, M>> AsVerifiable() =>
        Response.FlatMap(async r => new VerifiableHttpResponse<T, M>(r.StatusCode, await ParseContent()));

    public async Task<T> AsData()
    {
        await EnsureSuccess();
        return (await ParseContent()).Data.Value;
    }

    public async Task<M> AsMetadata()
    {
        await EnsureSuccess();
        return (await ParseContent()).Meta;
    }

    public async Task<bool> Check(Func<T, bool> condition) => await IsSuccess && condition(await AsData());
}
