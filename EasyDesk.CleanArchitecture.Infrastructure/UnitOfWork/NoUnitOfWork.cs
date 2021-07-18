using EasyDesk.CleanArchitecture.Application.Data;
using EasyDesk.CleanArchitecture.Application.Responses;
using EasyDesk.Tools;
using System;
using System.Threading.Tasks;
using static EasyDesk.CleanArchitecture.Application.Responses.ResponseImports;

namespace EasyDesk.CleanArchitecture.Infrastructure.UnitOfWork
{
    public class NoUnitOfWork : UnitOfWorkBase<NoTransaction>
    {
        protected override Task<NoTransaction> BeginTransaction() => Task.FromResult(new NoTransaction());

        protected override Task<Response<Nothing>> CommitTransaction(NoTransaction transaction) => OkAsync;

        protected override Task<Response<Nothing>> SaveWithinTransaction(NoTransaction transaction) => OkAsync;
    }

    public sealed class NoTransaction : IDisposable
    {
        public void Dispose() { }
    }
}
