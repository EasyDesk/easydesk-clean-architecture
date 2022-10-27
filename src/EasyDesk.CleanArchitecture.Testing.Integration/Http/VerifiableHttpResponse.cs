using EasyDesk.CleanArchitecture.Web.Dto;
using System.Net;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public record VerifiableHttpResponse<T>(
    ResponseDto<T> Content,
    HttpStatusCode StatusCode);
