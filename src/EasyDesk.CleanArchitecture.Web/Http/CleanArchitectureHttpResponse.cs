using EasyDesk.CleanArchitecture.Web.Dto;

namespace EasyDesk.CleanArchitecture.Web.Http;

public record CleanArchitectureHttpResponse<T>(ResponseDto<T> Content, HttpResponseMessage HttpResponseMessage);
