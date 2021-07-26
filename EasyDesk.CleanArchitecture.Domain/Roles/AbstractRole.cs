using EasyDesk.Tools;
using EasyDesk.Tools.Collections;
using EasyDesk.Tools.Options;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using static EasyDesk.Tools.Collections.ImmutableCollections;

namespace EasyDesk.CleanArchitecture.Domain.Roles
{
    public abstract class AbstractRole<T> : IRole
        where T : AbstractRole<T>
    {
        private static readonly IEqualityComparer<AbstractRole<T>> _equalityComparer =
            EqualityComparers.FromProperties<AbstractRole<T>>(x => x.Id);

        private static readonly Lazy<IImmutableDictionary<string, T>> _rolesById;

        static AbstractRole()
        {
            var roleType = typeof(T);
            var properties = roleType
                .GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
            _rolesById = new(() => Map(properties
                .Where(p => p.PropertyType.Equals(roleType))
                .Select(p => p.GetValue(null) as T)
                .Select(r => (r.ToString(), r))), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public AbstractRole(RoleId id)
        {
            Id = id;
        }

        public RoleId Id { get; }

        public static T From(string roleId) => _rolesById
            .Value
            .GetOption(roleId)
            .OrElseThrow(() => new ArgumentException($"The given RoleId is not a valid {typeof(T).Name}", nameof(roleId)));

        public static T FromRoleId(RoleId roleId) => From(roleId.ToString());

        public static IEnumerable<T> List() => _rolesById.Value.Values;

        public override bool Equals(object other) => _equalityComparer.Equals(this, other as T);

        public override int GetHashCode() => _equalityComparer.GetHashCode(this);

        public override string ToString() => Id.ToString();
    }
}
