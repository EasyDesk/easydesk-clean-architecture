using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public class GlobalErrorMapper
{
    private readonly IImmutableDictionary<Type, VersionedErrorMapper> _errorMappers;

    public GlobalErrorMapper(IImmutableDictionary<Type, VersionedErrorMapper> errorMappers)
    {
        _errorMappers = errorMappers;
    }

    public Error MapError(Error error, Option<ApiVersion> requestVersion)
    {
        var errorType = error.GetType();

        return _errorMappers
            .GetOption(errorType)
            .Map(m => m.MapError(error, requestVersion))
            .OrElse(error);
    }
}
