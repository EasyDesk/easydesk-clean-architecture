using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections.Immutable;

namespace EasyDesk.CleanArchitecture.Application.Authorization.RoleBased;

public interface IAgentRolesProvider
{
    Task<IFixedSet<Role>> GetRolesForAgent(Agent agent);
}
