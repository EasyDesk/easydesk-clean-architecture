using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using System.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IAgentPermissionsProvider
{
    Task<IImmutableSet<Permission>> GetPermissionsForAgent(Agent agent);
}
