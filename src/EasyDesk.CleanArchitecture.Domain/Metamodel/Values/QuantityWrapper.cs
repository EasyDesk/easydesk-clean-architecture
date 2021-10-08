using System;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values
{
    /// <summary>
    /// Extend this class to declare a wrapper of a physical quantity.
    /// In order to access the wrapped value the user of your class
    /// will need to call one of your defined properties.
    /// You should define properties for each unit of measuring
    /// that compute their value based on the wrapped value.
    /// </summary>
    /// <typeparam name="T">The same record that extends this one.</typeparam>
    public abstract record QuantityWrapper<T>
        : AbstractValueWrapper<T>
        where T : IEquatable<T>
    {
        public QuantityWrapper(T value) : base(value)
        {
        }
    }
}
