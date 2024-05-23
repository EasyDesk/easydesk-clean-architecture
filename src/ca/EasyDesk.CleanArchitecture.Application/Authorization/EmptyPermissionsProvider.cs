using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

internal class EmptyPermissionsProvider : IAgentPermissionsProvider
{
    public Task<IFixedSet<Permission>> GetPermissionsForAgent(Agent agent) =>
        Task.FromResult(Set<Permission>());
}
