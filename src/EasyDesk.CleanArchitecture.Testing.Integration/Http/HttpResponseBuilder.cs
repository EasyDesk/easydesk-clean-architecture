using EasyDesk.CleanArchitecture.Web.Dto;
using Newtonsoft.Json;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpResponseBuilder
{
    private readonly HttpResponseMessage _httpResponseMessage;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public HttpResponseBuilder(HttpResponseMessage httpResponseMessage, JsonSerializerSettings jsonSerializerSettings)
    {
        _httpResponseMessage = httpResponseMessage;
        _jsonSerializerSettings = jsonSerializerSettings;
    }

    public async Task<VerifiableHttpResponse<T>> AsVerifiable<T>() =>
        new(
            _httpResponseMessage.StatusCode,
            await ParseContent<T>(_httpResponseMessage));

    public async Task<T> AsData<T>()
    {
        await EnsureSuccess();
        return (await ParseContent<T>(_httpResponseMessage)).Data;
    }

    public bool IsSuccess => _httpResponseMessage.IsSuccessStatusCode && _httpResponseMessage.Content is not null;

    public async Task EnsureSuccess()
    {
        if (!IsSuccess)
        {
            throw await HttpRequestUnexpectedFailureException.Create(_httpResponseMessage);
        }
    }

    public async Task<bool> Check<T>(Func<T, bool> condition) => IsSuccess && condition(await AsData<T>());

    private async Task<ResponseDto<T>> ParseContent<T>(HttpResponseMessage response)
    {
        var bodyAsJson = await response.Content.ReadAsStringAsync();
        try
        {
            return JsonConvert.DeserializeObject<ResponseDto<T>>(bodyAsJson, _jsonSerializerSettings);
        }
        catch (JsonException e)
        {
            throw new Exception($"Failed to parse response as {typeof(T).Name}. Content was:\n\n{bodyAsJson}", e);
        }
    }
}

public static class HttpResponseBuilderExtensions
{
    public static async Task<VerifiableHttpResponse<T>> AsVerifiable<T>(this Task<HttpResponseBuilder> builder) =>
        await (await builder).AsVerifiable<T>();

    public static async Task<T> AsData<T>(this Task<HttpResponseBuilder> builder) =>
        await (await builder).AsData<T>();

    public static async Task EnsureSuccess(this Task<HttpResponseBuilder> builder) =>
        await (await builder).EnsureSuccess();
}
