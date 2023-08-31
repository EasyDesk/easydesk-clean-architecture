using EasyDesk.CleanArchitecture.Application.Versioning;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Comparers;
using EasyDesk.Commons.Options;
using EasyDesk.Commons.Results;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.ErrorManagement;

public class VersionedErrorMapper
{
    private readonly Func<Error, Error> _unversionedMapper;
    private readonly ImmutableList<(ApiVersion Version, Func<Error, Error> Mapper)> _versionedMappers;

    public VersionedErrorMapper(IDictionary<ApiVersion, Func<Error, Error>> versionedMappers, Option<Func<Error, Error>> unversionedMapper)
    {
        _versionedMappers = versionedMappers
            .OrderBy(x => x.Key)
            .Select(x => (x.Key, x.Value))
            .ToImmutableList();
        _unversionedMapper = unversionedMapper | It;
    }

    public Error MapError(Error error, Option<ApiVersion> requestVersion)
    {
        var mapper = requestVersion
            .FlatMap(v => _versionedMappers.LastOption(m => m.Version.IsLessThanOrEqualTo(v)))
            .Map(m => m.Mapper)
            .OrElse(_unversionedMapper);

        return mapper(error);
    }
}
