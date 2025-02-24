using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Scopes;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Data;

public abstract class UnitOfWorkManager<T> : IUnitOfWorkManager where T : IDisposable
{
    private readonly ScopeManager<Option<T>> _scopeManager = new(None);

    protected Option<T> MainUnitOfWork => _scopeManager.AsEnumerable().FirstOrDefault();

    protected Option<T> CurrentUnitOfWork => _scopeManager.Current;

    public async Task<Result<R>> RunTransactionally<R>(AsyncFunc<Result<R>> action)
    {
        using var transaction = await CreateTransaction();
        using var scope = _scopeManager.OpenScope(Some(transaction));
        try
        {
            var result = await action();
            if (result.IsSuccess)
            {
                await Commit(transaction);
            }
            else
            {
                await WrapInPassThroughException(async () =>
                {
                    await Rollback(transaction);
                });
            }
            return result;
        }
        catch (PassThroughException e)
        {
            throw e.Inner;
        }
        catch
        {
            await Rollback(transaction);
            throw;
        }
    }

    protected abstract Task<T> CreateTransaction();

    protected abstract Task Commit(T transaction);

    protected abstract Task Rollback(T transaction);

    private Task WrapInPassThroughException(AsyncAction asyncAction)
    {
        try
        {
            return asyncAction();
        }
        catch (Exception e)
        {
            throw new PassThroughException(e);
        }
    }

    private class PassThroughException : Exception
    {
        public PassThroughException(Exception inner) : base(null, inner)
        {
        }

        public Exception Inner => InnerException!;
    }
}
