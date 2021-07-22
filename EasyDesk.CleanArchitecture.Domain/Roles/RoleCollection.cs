using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static EasyDesk.Tools.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Domain.Roles
{
    public record RoleCollection : IEnumerable<Role>
    {
        private readonly IImmutableSet<Role> _roles;

        public RoleCollection(IImmutableSet<Role> roles)
        {
            _roles = roles;
        }

        public static RoleCollection Empty { get; } = Create(Enumerable.Empty<Role>());

        public static RoleCollection Create(IEnumerable<Role> roles) => new(Set(roles));

        public static RoleCollection Create(params Role[] roles) => Create(roles.AsEnumerable());

        public RoleCollection Add(Role role) => new(_roles.Add(role));

        public RoleCollection Remove(Role role) => new(_roles.Remove(role));

        public bool Contains(Role role) => _roles.Contains(role);

        public IEnumerator<Role> GetEnumerator() => _roles.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
