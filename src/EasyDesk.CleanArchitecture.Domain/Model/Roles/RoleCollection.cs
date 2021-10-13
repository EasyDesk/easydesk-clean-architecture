using EasyDesk.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using static EasyDesk.Tools.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Domain.Model.Roles
{
    public record RoleCollection<T> : IEnumerable<T>
        where T : IRole
    {
        private static readonly IEqualityComparer<T> _roleComparer = EqualityComparers.FromProperties<T>(r => r.Id);

        private readonly IImmutableSet<T> _roles;

        private RoleCollection(IImmutableSet<T> roles)
        {
            _roles = roles;
        }

        public static RoleCollection<T> Empty { get; } = Create(Enumerable.Empty<T>());

        public static RoleCollection<T> Create(IEnumerable<T> roles) => new(Set(roles, _roleComparer));

        public static RoleCollection<T> Create(params T[] roles) => Create(roles.AsEnumerable());

        public RoleCollection<T> Add(T role) => new(_roles.Add(role));

        public RoleCollection<T> Remove(T role) => new(_roles.Remove(role));

        public bool Contains(T role) => _roles.Contains(role);

        public IEnumerator<T> GetEnumerator() => _roles.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
