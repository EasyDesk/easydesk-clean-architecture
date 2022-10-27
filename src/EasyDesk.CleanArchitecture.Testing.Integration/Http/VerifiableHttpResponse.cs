using EasyDesk.CleanArchitecture.Web.Dto;
using System.Net;
using System.Net.Http.Headers;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public record VerifiableHttpResponse<T>(
    ResponseDto<T> Content,
    HttpStatusCode StatusCode,
    HttpResponseHeaders Headers);
