﻿using EasyDesk.CleanArchitecture.Application.Cqrs;
using EasyDesk.CleanArchitecture.Application.Dispatching.Pipeline;
using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Domain.Metamodel;

namespace EasyDesk.CleanArchitecture.Application.DomainServices;

public class DomainConstraintViolationsHandlingStep<T, R> : IPipelineStep<T, R>
    where R : notnull
    where T : IReadWriteOperation
{
    public async Task<Result<R>> Run(T request, NextPipelineStep<R> next)
    {
        try
        {
            return await next();
        }
        catch (DomainConstraintException e)
        {
            return e.DomainErrors.Any() ? Errors.Multiple(e.DomainErrors.First(), e.DomainErrors.Skip(1)) : throw e;
        }
    }
}