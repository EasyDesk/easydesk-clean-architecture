namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

public abstract record AbstractValueWrapper<T, S>
    where T : IEquatable<T>
    where S : AbstractValueWrapper<T, S>
{
    public T Value { get; }

    protected AbstractValueWrapper(T value)
    {
        Value = value;
    }

    public sealed override string ToString() => StringRepresentation();

    protected virtual string StringRepresentation() => Value!.ToString()!;
}
