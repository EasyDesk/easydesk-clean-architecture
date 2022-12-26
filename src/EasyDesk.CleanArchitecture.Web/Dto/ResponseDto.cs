using System.Collections.Immutable;
using static EasyDesk.Tools.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record ResponseDto<T, M>(Option<T> Data, IImmutableList<ErrorDto> Errors, M Meta)
{
    public static ResponseDto<T, M> FromData(T data, M meta) =>
        new(Some(data), List<ErrorDto>(), meta);

    public static ResponseDto<T, M> FromErrors(IEnumerable<ErrorDto> errors, M meta) =>
        new(None, errors.ToEquatableList(), meta);

    public static ResponseDto<T, M> FromError(ErrorDto error, M meta) =>
        FromErrors(new[] { error }, meta);

    public static ResponseDto<T, M> FromResult(Result<T> result, M meta) => result.Match(
        success: t => FromData(t, meta),
        failure: e => FromErrors(ErrorDto.CreateErrorDtoList(e), meta));
}
