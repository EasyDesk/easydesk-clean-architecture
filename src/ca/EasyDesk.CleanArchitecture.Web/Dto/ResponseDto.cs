using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record ResponseDto<T, M>
{
    public Option<T> Data { get; init; }

    public required IFixedList<ErrorDto> Errors { get; init; }

    public Option<M> Meta { get; init; }

    public static ResponseDto<T, M> FromData(T data, Option<M> meta = default) =>
        new() { Data = Some(data), Errors = List<ErrorDto>(), Meta = meta };

    public static ResponseDto<T, M> FromData(T data, M meta) => FromData(data, Some(meta));

    public static ResponseDto<T, M> FromErrors(IEnumerable<ErrorDto> errors, Option<M> meta = default) =>
        new() { Data = None, Errors = errors.ToFixedList(), Meta = meta };

    public static ResponseDto<T, M> FromErrors(IEnumerable<ErrorDto> errors, M meta) => FromErrors(errors, Some(meta));

    public static ResponseDto<T, M> FromError(ErrorDto error, Option<M> meta = default) =>
        FromErrors(new[] { error }, meta);

    public static ResponseDto<T, M> FromError(ErrorDto error, M meta) => FromError(error, Some(meta));

    public static ResponseDto<T, M> FromResult(Result<T> result, Option<M> meta) => result.Match(
        success: t => FromData(t, meta),
        failure: e => FromErrors(ErrorDto.CreateErrorDtoList(e), meta));

    public static ResponseDto<T, M> FromResult(Result<T> result, M meta) => FromResult(result, Some(meta));
}
