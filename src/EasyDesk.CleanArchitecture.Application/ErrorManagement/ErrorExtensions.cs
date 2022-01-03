using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Results;
using EasyDesk.Tools;
using EasyDesk.Tools.Options;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public static class ErrorExtensions
{
    public static Response<T> OrElseError<T>(this Option<T> option, Func<Error> errorSupplier) => option.Match<Response<T>>(
        some: t => t,
        none: () => errorSupplier());

    public static Response<T> OrElseNotFound<T>(this Option<T> option) =>
        option.OrElseError(Errors.NotFound<T>);

    public static Response<T> ToResponse<T>(this Result<T> result) =>
        result.Match<Response<T>>(
            success: t => t,
            failure: e => new DomainErrorWrapper(e));

    public static Task<Response<T>> ThenToResponse<T>(this Task<Result<T>> result) =>
        result.Map(x => x.ToResponse());

    private static Func<T, Response<R>> AsApplicationError<T, R>(this Func<T, Result<R>> function) =>
        t => function(t).ToResponse();

    private static AsyncFunc<T, Response<R>> AsApplicationError<T, R>(this AsyncFunc<T, Result<R>> function) =>
        t => function(t).ThenToResponse();

    public static Response<T> Require<T>(this Response<T> result, Func<T, Result<Nothing>> mapper) =>
        result.Require(mapper.AsApplicationError());

    public static Task<Response<T>> RequireAsync<T>(this Response<T> result, AsyncFunc<T, Result<Nothing>> mapper) =>
        result.RequireAsync(mapper.AsApplicationError());

    public static Task<Response<T>> ThenRequire<T>(this Task<Response<T>> result, Func<T, Result<Nothing>> mapper) =>
        result.ThenRequire(mapper.AsApplicationError());

    public static Task<Response<T>> ThenRequireAsync<T>(this Task<Response<T>> result, AsyncFunc<T, Result<Nothing>> mapper) =>
        result.ThenRequireAsync(mapper.AsApplicationError());

    public static Response<R> FlatMap<T, R>(this Response<T> result, Func<T, Result<R>> mapper) =>
        result.FlatMap(mapper.AsApplicationError());

    public static Task<Response<R>> FlatMapAsync<T, R>(this Response<T> result, AsyncFunc<T, Result<R>> mapper) =>
        result.FlatMapAsync(mapper.AsApplicationError());

    public static Task<Response<R>> ThenFlatMap<T, R>(this Task<Response<T>> result, Func<T, Result<R>> mapper) =>
        result.ThenFlatMap(mapper.AsApplicationError());

    public static Task<Response<R>> ThenFlatMapAsync<T, R>(this Task<Response<T>> result, AsyncFunc<T, Result<R>> mapper) =>
        result.ThenFlatMapAsync(mapper.AsApplicationError());

    public static Response<T> Require<T>(this Result<T> result, Func<T, Response<Nothing>> mapper) =>
        result.ToResponse().Require(mapper);

    public static Task<Response<T>> RequireAsync<T>(this Result<T> result, AsyncFunc<T, Response<Nothing>> mapper) =>
        result.ToResponse().RequireAsync(mapper);

    public static Task<Response<T>> ThenRequire<T>(this Task<Result<T>> result, Func<T, Response<Nothing>> mapper) =>
        result.ThenToResponse().ThenRequire(mapper);

    public static Task<Response<T>> ThenRequireAsync<T>(this Task<Result<T>> result, AsyncFunc<T, Response<Nothing>> mapper) =>
        result.ThenToResponse().ThenRequireAsync(mapper);

    public static Response<R> FlatMap<T, R>(this Result<T> result, Func<T, Response<R>> mapper) =>
        result.ToResponse().FlatMap(mapper);

    public static Task<Response<R>> FlatMapAsync<T, R>(this Result<T> result, AsyncFunc<T, Response<R>> mapper) =>
        result.ToResponse().FlatMapAsync(mapper);

    public static Task<Response<R>> ThenFlatMap<T, R>(this Task<Result<T>> result, Func<T, Response<R>> mapper) =>
        result.ThenToResponse().ThenFlatMap(mapper);

    public static Task<Response<R>> ThenFlatMapAsync<T, R>(this Task<Result<T>> result, AsyncFunc<T, Response<R>> mapper) =>
        result.ThenToResponse().ThenFlatMapAsync(mapper);
}
