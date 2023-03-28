using EasyDesk.Commons.Observables;

namespace EasyDesk.CleanArchitecture.Application.Data;

public interface IUnitOfWork : IDisposable
{
    IAsyncObservable<Nothing> BeforeCommit { get; }

    IAsyncObservable<Nothing> AfterCommit { get; }

    Task Commit();

    Task Rollback();
}
