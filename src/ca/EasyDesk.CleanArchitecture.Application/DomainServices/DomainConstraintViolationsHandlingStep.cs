using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public sealed class DomainConstraintViolationsHandlingStep<T, R> : IPipelineStep<T, R>
    where T : IReadWriteOperation
{
    public bool IsForEachHandler => true;

    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        try
        {
            return await next();
        }
        catch (DomainConstraintException e)
        {
            if (e.DomainErrors.IsEmpty())
            {
                return Errors.Multiple(e.DomainErrors.First(), e.DomainErrors.Skip(1));
            }
            else
            {
                throw;
            }
        }
    }
}
