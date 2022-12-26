using EasyDesk.CleanArchitecture.Web.Dto;
using Newtonsoft.Json;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public class HttpPaginatedResponseWrapper
{
    private readonly HttpResponseMessage _httpResponseMessage;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public HttpPaginatedResponseWrapper(HttpResponseMessage httpResponseMessage, JsonSerializerSettings jsonSerializerSettings)
    {
        _httpResponseMessage = httpResponseMessage;
        _jsonSerializerSettings = jsonSerializerSettings;
    }

    public async Task<(int, int)> GetIndex()
    {
        var paginationMeta = (await ParseContent<object>()).Meta;
        return (paginationMeta.PageIndex, paginationMeta.PageCount);
    }

    public bool IsSuccess => _httpResponseMessage.IsSuccessStatusCode && _httpResponseMessage.Content is not null;

    public async Task EnsureSuccess()
    {
        if (!IsSuccess)
        {
            throw await HttpRequestUnexpectedFailureException.Create(_httpResponseMessage);
        }
    }

    public async Task<IEnumerable<T>> GetCollection<T>() => (await ParseContent<T>()).Data.Value;

    private async Task<ResponseDto<IEnumerable<T>, PaginationMetaDto>> ParseContent<T>()
    {
        var bodyAsJson = await _httpResponseMessage.Content.ReadAsStringAsync();
        try
        {
            return JsonConvert.DeserializeObject<ResponseDto<IEnumerable<T>, PaginationMetaDto>>(bodyAsJson, _jsonSerializerSettings);
        }
        catch (JsonException e)
        {
            throw new Exception($"Failed to parse response as paginated {typeof(T).Name}. Content was:\n\n{bodyAsJson}", e);
        }
    }
}
