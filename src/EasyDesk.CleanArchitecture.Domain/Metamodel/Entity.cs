using System;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel
{
    public abstract class Entity<T, TId> : IEquatable<T>
        where T : Entity<T, TId>
        where TId : IEquatable<TId>
    {
        internal abstract TId Identifier { get; }

        private Entity()
        {
        }

        public bool Equals(T other) => other?.Identifier?.Equals(Identifier) ?? false;

        public override bool Equals(object obj) => obj is T t && Equals(t);

        public override int GetHashCode() => HashCode.Combine(Identifier);

        public static bool operator ==(Entity<T, TId> left, T right) => left.Equals(right);

        public static bool operator !=(Entity<T, TId> left, T right) => !left.Equals(right);

        public static bool operator ==(T left, Entity<T, TId> right) => right.Equals(left);

        public static bool operator !=(T left, Entity<T, TId> right) => !right.Equals(left);

        public abstract class ExplicitId : Entity<T, TId>
        {
            public abstract TId Id { get; }

            internal sealed override TId Identifier => Id;
        }

        public abstract class DerivedId : Entity<T, TId>
        {
            protected abstract TId Id { get; }

            internal sealed override TId Identifier => Id;
        }
    }
}
