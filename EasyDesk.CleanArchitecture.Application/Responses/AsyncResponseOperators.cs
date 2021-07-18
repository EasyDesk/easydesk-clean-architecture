using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.Tools;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Responses
{
    public static partial class ResponseImports
    {
        public static async Task<Response<A>> ThenIfSuccess<A>(this Task<Response<A>> response, Action<A> action) =>
            (await response).IfSuccess(action);

        public static async Task<Response<A>> ThenIfSuccessAsync<A>(this Task<Response<A>> response, AsyncAction<A> action) =>
            await (await response).IfSuccessAsync(action);

        public static async Task<Response<A>> ThenIfFailure<A>(this Task<Response<A>> response, Action<Error> action) =>
            (await response).IfFailure(action);

        public static async Task<Response<A>> ThenIfFailureAsync<A>(this Task<Response<A>> response, AsyncAction<Error> action) =>
            await (await response).IfFailureAsync(action);

        public static async Task<Response<A>> ThenRequire<A>(this Task<Response<A>> response, Func<A, Response<Nothing>> requirement) =>
            (await response).Require(requirement);

        public static async Task<Response<A>> ThenRequireAsync<A>(this Task<Response<A>> response, AsyncFunc<A, Response<Nothing>> requirement) =>
            await (await response).RequireAsync(requirement);

        public static async Task<Response<B>> ThenMap<A, B>(this Task<Response<A>> response, Func<A, B> mapper) =>
            (await response).Map(mapper);

        public static async Task<Response<B>> ThenMapAsync<A, B>(this Task<Response<A>> response, AsyncFunc<A, B> mapper) =>
            await (await response).MapAsync(mapper);

        public static async Task<Response<A>> ThenMapError<A>(this Task<Response<A>> response, Func<Error, Error> mapper) =>
            (await response).MapError(mapper);

        public static async Task<Response<B>> ThenFlatMap<A, B>(this Task<Response<A>> response, Func<A, Response<B>> mapper) =>
            (await response).FlatMap(mapper);

        public static async Task<Response<B>> ThenFlatMapAsync<A, B>(this Task<Response<A>> response, AsyncFunc<A, Response<B>> mapper) =>
            await (await response).FlatMapAsync(mapper);
    }
}
