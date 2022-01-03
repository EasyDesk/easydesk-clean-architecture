using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Tools;
using EasyDesk.Tools.Results;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.Application.Responses;

public record Response<T> : ResultBase<T, Error>
{
    public Response(T value) : base(value)
    {
    }

    public Response(Error error) : base(error)
    {
    }

    public override string ToString() => Match(
        success: v => $"Success({v})",
        failure: e => $"Failure({e})");

    public static implicit operator Response<Nothing>(Response<T> response) => response.Map(_ => Nothing.Value);

    public static implicit operator Response<T>(T data) => Success(data);

    public static implicit operator Response<T>(Error error) => Failure<T>(error);

    public static Response<T> operator |(Response<T> a, Response<T> b) => a.Match(
        success: _ => a,
        failure: _ => b);

    public static Response<T> operator &(Response<T> a, Response<T> b) => a.Match(
        success: _ => b,
        failure: _ => a);
}
