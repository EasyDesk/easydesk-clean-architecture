using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.Commons.Collections.Immutable;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public class GlobalErrorMapper
{
    private readonly IFixedMap<Type, VersionedErrorMapper> _errorMappers;

    public GlobalErrorMapper(IFixedMap<Type, VersionedErrorMapper> errorMappers)
    {
        _errorMappers = errorMappers;
    }

    public Error MapError(Error error, Option<ApiVersion> requestVersion)
    {
        var errorType = error.GetType();

        return _errorMappers
            .Get(errorType)
            .Map(m => m.MapError(error, requestVersion))
            .OrElse(error);
    }
}
