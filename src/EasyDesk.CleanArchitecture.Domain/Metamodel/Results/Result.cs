﻿using EasyDesk.Tools;
using EasyDesk.Tools.Results;
using static EasyDesk.CleanArchitecture.Domain.Metamodel.Results.ResultImports;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Results
{
    public record Result<T> : ResultBase<T, DomainError>
    {
        public Result(T value) : base(value)
        {
        }

        public Result(DomainError error) : base(error)
        {
        }

        public static implicit operator Result<Nothing>(Result<T> result) => result.Map(_ => Nothing.Value);

        public static implicit operator Result<T>(T data) => Success(data);

        public static implicit operator Result<T>(DomainError error) => Failure<T>(error);

        public static Result<T> operator |(Result<T> a, Result<T> b) => a.Match(
            success: _ => a,
            failure: _ => b);

        public static Result<T> operator &(Result<T> a, Result<T> b) => a.Match(
            success: _ => b,
            failure: _ => a);
    }
}