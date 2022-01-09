using EasyDesk.CleanArchitecture.Domain.Metamodel;
using EasyDesk.CleanArchitecture.Domain.Metamodel.Results;
using EasyDesk.Tools.Options;
using System;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Domain.Utils;

public static class DomainErrorExtensions
{
    public static Result<T> OrElseError<T>(this Option<T> option, Func<DomainError> errorFactory) => option.Match<Result<T>>(
        some: t => t,
        none: () => errorFactory());

    public static Task<Result<T>> ThenOrElseError<T>(this Task<Option<T>> option, Func<DomainError> errorFactory) => option.ThenMatch<T, Result<T>>(
        some: t => t,
        none: () => errorFactory());
}
