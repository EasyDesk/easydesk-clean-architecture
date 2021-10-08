using System;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values
{
    /// <summary>
    /// Extend this class to declare a wrapper of a pure value.
    /// In order to access the wrapped value the users of your class
    /// will be able to use your defined properties and methods
    /// as well as using directly this class as the wrapped value,
    /// thanks to the implicit conversion.
    /// </summary>
    /// <typeparam name="T">The same record that extends this one.</typeparam>
    public abstract record ValueWrapper<T>
        : AbstractValueWrapper<T>
        where T : IEquatable<T>
    {
        public ValueWrapper(T value) : base(value)
        {
        }

        public static implicit operator T(ValueWrapper<T> wrapper) => wrapper.Value;
    }
}
