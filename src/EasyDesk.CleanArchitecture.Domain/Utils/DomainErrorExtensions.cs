using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Results;
using EasyDesk.Tools.Options;
using System;

namespace EasyDesk.CleanArchitecture.Domain.Utils;

public static class DomainErrorExtensions
{
    public static Result<T> OrElseError<T>(this Option<T> option, Func<DomainError> errorFactory)
    {
        return option.Match<Result<T>>(
            some: t => t,
            none: () => errorFactory());
    }
}
