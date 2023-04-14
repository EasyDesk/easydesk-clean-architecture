using System.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public record UserInfo(UserId UserId, IImmutableDictionary<string, string> Attributes)
{
    public static UserInfo Create(UserId userId) =>
        Create(userId, Map<string, string>());

    public static UserInfo Create(UserId userId, IImmutableDictionary<string, string> claims) =>
        new(userId, claims);
}
