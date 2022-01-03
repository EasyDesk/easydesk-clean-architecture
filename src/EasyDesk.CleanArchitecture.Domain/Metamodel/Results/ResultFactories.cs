using EasyDesk.Tools;
using System;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Results;

public static partial class ResultImports
{
    public static Result<Nothing> Ok { get; } = Nothing.Value;

    public static Result<T> Success<T>(T data) => new(data);

    public static Result<T> Failure<T>(DomainError error) => new(error);

    public static Result<Nothing> RequireTrue(bool condition, Func<DomainError> errorFactory) =>
        condition ? Ok : errorFactory();

    public static Result<Nothing> RequireFalse(bool condition, Func<DomainError> errorFactory) =>
        RequireTrue(!condition, errorFactory);
}
