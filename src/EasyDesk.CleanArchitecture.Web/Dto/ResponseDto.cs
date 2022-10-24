using EasyDesk.Tools.Collections;
using System.Collections.Immutable;
using static EasyDesk.Tools.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record ResponseDto<T>(T Data, IImmutableList<ErrorDto> Errors, object Meta)
{
    public static ResponseDto<T> FromData(T data, object meta = null) =>
        new(data, List<ErrorDto>(), meta ?? Nothing.Value);

    public static ResponseDto<T> FromErrors(IEnumerable<ErrorDto> errors, object meta = null) =>
        new(default, List(errors), meta ?? Nothing.Value);

    public static ResponseDto<T> FromError(ErrorDto error, object meta = null) =>
        FromErrors(EnumerableUtils.Items(error), meta);

    public static ResponseDto<T> FromResult(Result<T> result, object meta = null) => result.Match(
        success: t => FromData(t, meta),
        failure: e => FromErrors(ErrorDto.CreateErrorDtoList(e), meta));
}
