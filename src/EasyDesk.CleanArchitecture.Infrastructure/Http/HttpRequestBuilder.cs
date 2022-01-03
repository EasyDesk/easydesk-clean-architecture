using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using System.Net.Http;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Infrastructure.Http;

public record HttpResponseDto<T>(T Data, HttpErrorDto Error);

public record HttpErrorDto(string Message, string ErrorCode);

public class HttpRequestBuilder
{
    private readonly HttpClient _httpClient;
    private readonly HttpRequest _request;

    public HttpRequestBuilder(HttpClient httpClient, HttpRequest request)
    {
        _httpClient = httpClient;
        _request = request;
    }

    protected async Task<HttpResponseDto<T>> MakeRequest<T>()
    {
        var response = await _request(_httpClient);
        return await response.Content.ReadAsAsync<HttpResponseDto<T>>();
    }

    public async Task<Response<T>> As<T>()
    {
        return await MakeRequest<T>().Map(DtoToObjectResponse);
    }

    public async Task<Response<Nothing>> AsEmpty()
    {
        return await As<Nothing>();
    }

    private Response<T> DtoToObjectResponse<T>(HttpResponseDto<T> responseDto)
    {
        if (responseDto.Error is not null)
        {
            return GetError(responseDto);
        }
        return responseDto.Data;
    }

    private Error GetError<T>(HttpResponseDto<T> responseDto)
    {
        return DtoToError(responseDto.Error);
    }

    private Error DtoToError(HttpErrorDto errorDto)
    {
        return Errors.Generic(
            errorDto.Message,
            errorDto.ErrorCode);
    }
}
