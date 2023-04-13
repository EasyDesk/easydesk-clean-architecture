using System.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public record UserInfo(string UserId, IImmutableDictionary<string, string> Attributes)
{
    public static UserInfo Create(string userId) =>
        Create(userId, Map<string, string>());

    public static UserInfo Create(string userId, IImmutableDictionary<string, string> claims) =>
        new(userId, claims);
}
