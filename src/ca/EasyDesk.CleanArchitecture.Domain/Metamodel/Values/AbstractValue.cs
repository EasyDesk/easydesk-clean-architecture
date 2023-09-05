namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

public abstract record AbstractValue<T, TSelf>
    where T : notnull, IEquatable<T>
    where TSelf : AbstractValue<T, TSelf>, IValue<T>
{
    protected T InnerValue { get; }

    protected AbstractValue(T value) : this(value, process: true)
    {
    }

    protected AbstractValue(T value, bool process)
    {
        if (process)
        {
            InnerValue = IValue<T>.Companion<TSelf>.ProcessAndValidate(value);
        }
        else
        {
            InnerValue = value;
        }
    }

    public sealed override string ToString() => StringRepresentation();

    protected virtual string StringRepresentation() => InnerValue.ToString()!;
}
