using EasyDesk.CleanArchitecture.Application.ContextProvider;

namespace EasyDesk.CleanArchitecture.Application.Auditing;

public record IdentityQuery(Realm Realm, IdentityId IdentityId);
