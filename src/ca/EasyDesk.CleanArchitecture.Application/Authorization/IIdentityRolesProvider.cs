using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IIdentityRolesProvider
{
    Task<IImmutableSet<Role>> GetRolesForIdentity(Identity identity);
}
