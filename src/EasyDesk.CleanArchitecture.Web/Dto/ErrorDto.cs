using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.Tools;
using System;
using System.Linq.Expressions;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record ErrorDto(string Code, string Detail, object Meta)
{
    public static ErrorDto FromError(Error error) =>
        error switch
        {
            // TODO: implement all cases
            NotFoundError(var type) =>
                new(Errors.Codes.NotFound, $"Unable to find entity of type {{{type.Name}}}", new { EntityType = type.Name }),
            DomainErrorWrapper(var domainError) =>
                new(domainError.GetType().Name, $"Domain Error: {domainError.GetType().Name}", domainError),
            _ => throw new ArgumentException("Can't convert to single error.", nameof(error)),
        };
}
