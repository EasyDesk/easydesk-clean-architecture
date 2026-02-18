using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

public abstract record AbstractValue<T, TSelf>
    where T : notnull, IEquatable<T>
    where TSelf : AbstractValue<T, TSelf>, IValue<T>
{
    protected T InnerValue { get; }

    protected AbstractValue(T value) : this(value, validate: true)
    {
    }

    protected AbstractValue(T value, bool validate)
    {
        InnerValue = validate ? IValue<T>.Companion<TSelf>.Validate(value) : value;
    }

    public static bool IsValid(T value) => ValidateToOption(value).IsPresent;

    public static Option<T> ValidateToOption(T value) => IValue<T>.Companion<TSelf>.ValidateToOption(value);

    public sealed override string ToString() => StringRepresentation();

    protected virtual string StringRepresentation() => InnerValue.ToString()!;
}
