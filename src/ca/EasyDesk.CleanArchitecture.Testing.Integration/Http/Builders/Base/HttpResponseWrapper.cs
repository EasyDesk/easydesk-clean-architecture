using EasyDesk.CleanArchitecture.Web.Dto;
using EasyDesk.Commons.Tasks;
using System.Text.Json;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;

public class HttpResponseWrapper<T, M>
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly AsyncCache<ImmutableHttpResponseMessage> _response;

    public HttpResponseWrapper(AsyncFunc<ImmutableHttpResponseMessage> httpResponseMessage, JsonSerializerOptions jsonSerializerOptions)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
        _response = new(httpResponseMessage);
    }

    public async Task<ImmutableHttpResponseMessage> GetResponse() => await _response.Get();

    public Task<bool> IsSuccess() => GetResponse().Map(r => r.IsSuccessStatusCode && r.Content is not null);

    public async Task EnsureSuccess()
    {
        if (await IsSuccess())
        {
            return;
        }

        throw new HttpRequestUnexpectedFailureException(await GetResponse());
    }

    public async Task EnsureFailure()
    {
        if (!await IsSuccess())
        {
            return;
        }

        throw new HttpRequestUnexpectedSuccessException(await GetResponse());
    }

    private async Task<ResponseDto<T, M>?> ParseNullableContent()
    {
        var bodyAsJson = (await GetResponse()).Content.AsString();
        try
        {
            return JsonSerializer.Deserialize<ResponseDto<T, M>>(bodyAsJson, _jsonSerializerOptions);
        }
        catch (JsonException e)
        {
            throw new InvalidDataException($"Failed to parse response as {typeof(T).Name}. Content was:\n\n{bodyAsJson}", e);
        }
    }

    private async Task<ResponseDto<T, M>> ParseContent() => await ParseNullableContent() ?? throw new InvalidOperationException($"Response was successful but the body was empty. Expected a {typeof(T).Name}.");

    public Task<VerifiableHttpResponse<T, M>> AsVerifiable() =>
        GetResponse().FlatMap(async r => new VerifiableHttpResponse<T, M>(r.StatusCode, await ParseNullableContent()));

    public async Task<T> AsData()
    {
        await EnsureSuccess();
        return (await ParseContent()).Data.Value;
    }

    public async Task<M> AsMetadata()
    {
        return (await ParseContent()).Meta.Value;
    }
}
