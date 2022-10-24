using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Web.Dto;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Mime;

namespace EasyDesk.CleanArchitecture.Web.Http;

public class CleanArchitectureHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _settings;

    public CleanArchitectureHttpClient(HttpClient httpClient, JsonSettingsConfigurator jsonSettingsConfigurator)
    {
        _httpClient = httpClient;
        _settings = jsonSettingsConfigurator.CreateSettings();
    }

    public async Task<CleanArchitectureHttpResponse<TResponse>> Post<TBody, TResponse>(string requestUri, TBody body)
    {
        var bodyAsJson = JsonConvert.SerializeObject(body, Formatting.None, _settings);
        var content = new StringContent(bodyAsJson, null, MediaTypeNames.Application.Json);
        return await MakeRequest<TResponse>(http => http.PostAsync(requestUri, content));
    }

    private async Task<CleanArchitectureHttpResponse<T>> MakeRequest<T>(AsyncFunc<HttpClient, HttpResponseMessage> request)
    {
        var httpResponse = await request(_httpClient);
        var bodyAsJson = await httpResponse.Content.ReadAsStringAsync();
        var dto = JsonConvert.DeserializeObject<ResponseDto<T>>(bodyAsJson);
        return new(dto, httpResponse);
    }

    public CleanArchitectureHttpClient WithDefaultHeaders(Action<HttpHeaders> configureHeaders)
    {
        configureHeaders(_httpClient.DefaultRequestHeaders);
        return this;
    }
}
