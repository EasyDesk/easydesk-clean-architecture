using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using System.Collections.Generic;
using System.Linq;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record ResponseDto<T>(T Data, object Meta, IEnumerable<ErrorDto> Errors)
{
    public static ResponseDto<T> FromData(T data, object meta = null) => new(data, meta ?? Nothing.Value, Enumerable.Empty<ErrorDto>());

    public static ResponseDto<T> FromErrors(IEnumerable<ErrorDto> errors, object meta = null) => new(default, meta ?? Nothing.Value, errors);

    public static ResponseDto<T> FromError(ErrorDto error, object meta = null) => FromErrors(Some(error), meta);

    public static ResponseDto<T> FromResponse(Response<T> response, object meta = null) => response.Match(
        success: t => FromData(t, meta),
        failure: e => FromErrors(ErrorDto.CreateErrorDtoList(e), meta));
}
