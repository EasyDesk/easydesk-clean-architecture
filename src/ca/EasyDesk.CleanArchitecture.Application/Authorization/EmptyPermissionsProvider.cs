using EasyDesk.CleanArchitecture.Application.Authorization.Model;
using EasyDesk.CleanArchitecture.Application.ContextProvider;
using System.Collections.Immutable;
using static EasyDesk.Commons.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Application.Authorization;

internal class EmptyPermissionsProvider : IAgentPermissionsProvider
{
    public Task<IImmutableSet<Permission>> GetPermissionsForAgent(Agent agent) =>
        Task.FromResult(Set<Permission>());
}
