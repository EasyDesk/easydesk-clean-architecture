using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

public interface IAgentPermissionsProvider
{
    Task<IFixedSet<Permission>> GetPermissionsForAgent(Agent agent);
}
