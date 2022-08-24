namespace EasyDesk.CleanArchitecture.Web.Dto;

public record ResponseDto<T>(Option<T> Data, IEnumerable<ErrorDto> Errors, object Meta)
{
    public static ResponseDto<T> FromData(T data, object meta = null) => new(Some(data), Enumerable.Empty<ErrorDto>(), meta ?? Nothing.Value);

    public static ResponseDto<T> FromErrors(IEnumerable<ErrorDto> errors, object meta = null) => new(None, errors, meta ?? Nothing.Value);

    public static ResponseDto<T> FromError(ErrorDto error, object meta = null) => FromErrors(Some(error), meta);

    public static ResponseDto<T> FromResult(Result<T> result, object meta = null) => result.Match(
        success: t => FromData(t, meta),
        failure: e => FromErrors(ErrorDto.CreateErrorDtoList(e), meta));
}
