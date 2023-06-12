using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization.Model;

public record AuthorizationInfo(IImmutableSet<Permission> Permissions);
