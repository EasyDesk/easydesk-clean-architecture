using EasyDesk.CleanArchitecture.Application.Authentication;

namespace EasyDesk.CleanArchitecture.Application.Auditing;

public record IdentityQuery(Realm Realm, IdentityId IdentityId);
