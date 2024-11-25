using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record ResponseDto<T, M>
{
    public required Option<T> Data { get; init; }

    public required IFixedList<ErrorDto> Errors { get; init; }

    public required M Meta { get; init; }

    public static ResponseDto<T, M> FromData(T data, M meta) =>
        new() { Data = Some(data), Errors = List<ErrorDto>(), Meta = meta };

    public static ResponseDto<T, M> FromErrors(IEnumerable<ErrorDto> errors, M meta) =>
        new() { Data = None, Errors = errors.ToFixedList(), Meta = meta };

    public static ResponseDto<T, M> FromError(ErrorDto error, M meta) =>
        FromErrors(new[] { error }, meta);

    public static ResponseDto<T, M> FromResult(Result<T> result, M meta) => result.Match(
        success: t => FromData(t, meta),
        failure: e => FromErrors(ErrorDto.CreateErrorDtoList(e), meta));
}
