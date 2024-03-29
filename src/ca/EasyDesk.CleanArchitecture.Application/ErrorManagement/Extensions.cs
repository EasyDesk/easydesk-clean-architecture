﻿using EasyDesk.CleanArchitecture.Domain.Metamodel.Repositories;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public static class Extensions
{
    public static Task<Result<T>> OrElseNotFound<T>(this IAggregateView<T> view) =>
        view.OrElseError(Errors.NotFound);

    public static Task<Result<T>> ThenOrElseNotFound<T>(this Task<Option<T>> task) =>
        task.ThenOrElseError(Errors.NotFound);
}
