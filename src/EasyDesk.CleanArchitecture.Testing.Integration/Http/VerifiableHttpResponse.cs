using EasyDesk.CleanArchitecture.Web.Dto;
using System.Net;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public record VerifiableHttpResponse<T, M>(
    HttpStatusCode StatusCode,
    ResponseDto<T, M>? Content)
    where T : notnull;
