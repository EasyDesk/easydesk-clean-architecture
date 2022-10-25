using EasyDesk.CleanArchitecture.Web.Dto;
using Newtonsoft.Json;

namespace EasyDesk.CleanArchitecture.Web.Http;

public class HttpRequestBuilder
{
    private readonly HttpClient _httpClient;
    private readonly HttpRequestMessage _request;

    public HttpRequestBuilder(HttpClient httpClient, HttpRequestMessage request)
    {
        _httpClient = httpClient;
        _request = request;
    }

    public async Task<CleanArchitectureHttpResponse<T>> As<T>()
    {
        var httpResponse = await _httpClient.SendAsync(_request);
        var bodyAsJson = await httpResponse.Content.ReadAsStringAsync();
        var dto = JsonConvert.DeserializeObject<ResponseDto<T>>(bodyAsJson);
        return new(dto, httpResponse);
    }

    public Task<CleanArchitectureHttpResponse<Nothing>> AsEmpty() => As<Nothing>();
}
