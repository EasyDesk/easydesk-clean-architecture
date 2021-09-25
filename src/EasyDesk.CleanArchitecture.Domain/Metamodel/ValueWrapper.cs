using System;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel
{
    public abstract record ValueWrapper<T>
        where T : IEquatable<T>
    {
        protected T Value { get; }

        public ValueWrapper(T value)
        {
            Validate(value);
            Value = value;
        }

        public static implicit operator T(ValueWrapper<T> wrapper) => wrapper.Value;

        protected abstract void Validate(T value);
    }
}
