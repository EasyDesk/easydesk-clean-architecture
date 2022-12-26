using EasyDesk.CleanArchitecture.Web.Dto;
using Newtonsoft.Json;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpResponseWrapper
{
    private readonly HttpResponseMessage _httpResponseMessage;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public HttpResponseWrapper(HttpResponseMessage httpResponseMessage, JsonSerializerSettings jsonSerializerSettings)
    {
        _httpResponseMessage = httpResponseMessage;
        _jsonSerializerSettings = jsonSerializerSettings;
    }

    public async Task<VerifiableHttpResponse<T, Nothing>> AsVerifiable<T>() =>
        new(
            _httpResponseMessage.StatusCode,
            await ParseContent<T>());

    public async Task<T> AsData<T>()
    {
        await EnsureSuccess();
        return (await ParseContent<T>()).Data.Value;
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

    private async Task<ResponseDto<T, Nothing>> ParseContent<T>()
    {
        var bodyAsJson = await _httpResponseMessage.Content.ReadAsStringAsync();
        try
        {
            return JsonConvert.DeserializeObject<ResponseDto<T, Nothing>>(bodyAsJson, _jsonSerializerSettings);
        }
        catch (JsonException e)
        {
            throw new Exception($"Failed to parse response as {typeof(T).Name}. Content was:\n\n{bodyAsJson}", e);
        }
    }
}

public static partial class HttpResponseBuilderExtensions
{
    public static async Task<VerifiableHttpResponse<T, Nothing>> AsVerifiable<T>(this Task<HttpResponseWrapper> builder) =>
        await (await builder).AsVerifiable<T>();

    public static async Task<T> AsData<T>(this Task<HttpResponseWrapper> builder) =>
        await (await builder).AsData<T>();

    public static async Task EnsureSuccess(this Task<HttpResponseWrapper> builder) =>
        await (await builder).EnsureSuccess();
}
