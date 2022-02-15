using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using EasyDesk.Tools.Options;
using System.Collections.Generic;
using System.Linq;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record ResponseDto<T>(Option<T> Data, IEnumerable<ErrorDto> Errors, object Meta)
{
    public static ResponseDto<T> FromData(T data, object meta = null) => new(Some(data), Enumerable.Empty<ErrorDto>(), meta ?? Nothing.Value);

    public static ResponseDto<T> FromErrors(IEnumerable<ErrorDto> errors, object meta = null) => new(None, errors, meta ?? Nothing.Value);

    public static ResponseDto<T> FromError(ErrorDto error, object meta = null) => FromErrors(Some(error), meta);

    public static ResponseDto<T> FromResponse(Response<T> response, object meta = null) => response.Match(
        success: t => FromData(t, meta),
        failure: e => FromErrors(ErrorDto.CreateErrorDtoList(e), meta));
}
