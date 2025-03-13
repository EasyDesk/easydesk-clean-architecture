using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using EasyDesk.Commons.Scopes;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Data;

public abstract class UnitOfWorkManager<T> : IUnitOfWorkManager where T : IDisposable
{
    private readonly ScopeManager<Option<T>> _scopeManager = new(None);

    protected Option<T> CurrentUnitOfWork => _scopeManager.Current;

    public async Task<Result<R>> RunTransactionally<R>(AsyncFunc<Result<R>> action)
    {
        using var transaction = await CreateTransaction();
        using var scope = _scopeManager.OpenScope(Some(transaction));
        Result<R>? result;
        try
        {
            result = await action();
            if (result.Value.IsSuccess)
            {
                await Commit(transaction);
            }
        }
        catch
        {
            await Rollback(transaction);
            throw;
        }
        if (result.Value.IsFailure)
        {
            await Rollback(transaction);
        }
        return result.Value;
    }

    protected abstract Task<T> CreateTransaction();

    protected abstract Task Commit(T transaction);

    protected abstract Task Rollback(T transaction);
}
