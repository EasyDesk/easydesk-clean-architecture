using EasyDesk.Tools;
using EasyDesk.Tools.Observables;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Data;

public interface IUnitOfWork : IDisposable
{
    IAsyncObservable<Nothing> BeforeCommit { get; }

    IAsyncObservable<Nothing> AfterCommit { get; }

    Task Commit();

    Task Rollback();
}
