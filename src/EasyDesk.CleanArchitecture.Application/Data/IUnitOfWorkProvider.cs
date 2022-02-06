using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using EasyDesk.Tools.Options;
using System;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.Application.Data;

public interface IUnitOfWorkProvider
{
    Option<IUnitOfWork> CurrentUnitOfWork { get; }

    Task<IUnitOfWork> BeginUnitOfWork();
}

public static class UnitOfWorkProviderExtensions
{
    public static async Task<Response<T>> RunTransactionally<T>(this IUnitOfWorkProvider unitOfWorkProvider, AsyncFunc<Response<T>> action)
    {
        using var unitOfWork = await unitOfWorkProvider.BeginUnitOfWork();
        try
        {
            var response = await action();
            await response.MatchAsync(
                success: _ => unitOfWork.Commit(),
                failure: _ => unitOfWork.Rollback());
            return response;
        }
        catch (AfterCommitException)
        {
            throw;
        }
        catch (Exception)
        {
            await unitOfWork.Rollback();
            throw;
        }
    }

    public static async Task<Response<T>> RunTransactionally<T>(this IUnitOfWorkProvider unitOfWorkProvider, AsyncFunc<T> action) =>
        await unitOfWorkProvider.RunTransactionally(async () => Success(await action()));

    public static async Task<Response<Nothing>> RunTransactionally(this IUnitOfWorkProvider unitOfWorkProvider, AsyncAction action) =>
        await unitOfWorkProvider.RunTransactionally(() => Functions.Execute(action));
}
