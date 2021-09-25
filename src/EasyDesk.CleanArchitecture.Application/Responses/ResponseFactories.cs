using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Tools;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Responses
{
    public static partial class ResponseImports
    {
        public static Response<Nothing> Ok { get; } = Nothing.Value;

        public static Task<Response<Nothing>> OkAsync { get; } = Task.FromResult<Response<Nothing>>(Nothing.Value);

        public static Response<T> Success<T>(T data) => new(data);

        public static Response<T> Failure<T>(Error error) => new(error);

        public static Response<Nothing> RequireTrue(bool condition, Func<Error> errorFactory) =>
            condition ? Ok : errorFactory();

        public static Response<Nothing> RequireFalse(bool condition, Func<Error> errorFactory) =>
            RequireTrue(!condition, errorFactory);
    }
}
