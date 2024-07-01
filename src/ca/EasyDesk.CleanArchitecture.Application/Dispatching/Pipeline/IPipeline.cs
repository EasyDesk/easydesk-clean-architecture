using EasyDesk.Commons.Results;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;

public interface IPipeline
{
    Task<Result<R>> Run<T, R>(T request, AsyncFunc<T, Result<R>> action);
}
