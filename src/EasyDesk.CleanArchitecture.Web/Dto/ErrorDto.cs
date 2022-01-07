using EasyDesk.CleanArchitecture.Application.ErrorManagement;
using EasyDesk.CleanArchitecture.Application.Mapping;
using EasyDesk.Tools;
using System;
using System.Linq.Expressions;

namespace EasyDesk.CleanArchitecture.Web.Dto;

public record ErrorDto(string Message, string ErrorCode, object Parameters);

public abstract class ErrorDtoMappingBase<T> : SimpleMapping<T, ErrorDto>
    where T : Error
{
    public ErrorDtoMappingBase(Expression<Func<T, object>> parameters)
    {
        AddCtorParam(nameof(ErrorDto.Parameters), parameters);
    }
}

public class DomainErrorToErrorDto : ErrorDtoMappingBase<DomainErrorWrapper>
{
    public DomainErrorToErrorDto() : base(src => src.Error)
    {
    }
}

public class DomainErrorsToErrorDto : ErrorDtoMappingBase<DomainErrorsWrapper>
{
    public DomainErrorsToErrorDto() : base(src => src.Errors) // TODO: understand better and improve
    {
    }
}

public class NotFoundErrorToErrorDto : ErrorDtoMappingBase<NotFoundError>
{
    private record NotFoundParameters(string EntityType);

    public NotFoundErrorToErrorDto() : base(src => new NotFoundParameters(src.EntityType.Name))
    {
    }
}

public class GenericErrorToErrorDto : ErrorDtoMappingBase<GenericError>
{
    public GenericErrorToErrorDto() : base(src => src.Parameters)
    {
    }
}

public class InternalErrorToErrorDto : ErrorDtoMappingBase<InternalError>
{
    public InternalErrorToErrorDto() : base(_ => Nothing.Value)
    {
    }
}

public class ForbiddenErrorToErrorDto : ErrorDtoMappingBase<ForbiddenError>
{
    private record ForbiddenParameters(string Reason);

    public ForbiddenErrorToErrorDto() : base(src => new ForbiddenParameters(src.Reason))
    {
    }
}

public class PropertyValidationErrorToErrorDto : ErrorDtoMappingBase<PropertyValidationError>
{
    private record PropertyValidationParameters(string PropertyName, string ErrorMessage);

    public PropertyValidationErrorToErrorDto()
        : base(src => new PropertyValidationParameters(src.PropertyName, src.ErrorMessage))
    {
    }
}
