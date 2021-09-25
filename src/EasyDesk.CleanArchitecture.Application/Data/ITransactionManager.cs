using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using EasyDesk.Tools.Observables;
using EasyDesk.Tools.Options;
using System;
using System.Threading.Tasks;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Data
{
    public interface ITransactionManager : IDisposable
    {
        IAsyncObservable<BeforeCommitContext> BeforeCommit { get; }

        IAsyncObservable<AfterCommitContext> AfterCommit { get; }

        Task Begin();

        Task<Response<Nothing>> Commit();
    }

    public class BeforeCommitContext
    {
        public Option<Error> Error { get; private set; } = None;

        public void CancelCommit() => CancelCommit(Errors.Generic(Errors.Codes.Internal, "This unit of work was canceled"));

        public void CancelCommit(Error error)
        {
            Error = Some(error);
        }
    }

    public class AfterCommitContext
    {
        public AfterCommitContext(Option<Error> error)
        {
            Error = error;
        }

        public bool Successful => Error.IsAbsent;

        public Option<Error> Error { get; }
    }
}
