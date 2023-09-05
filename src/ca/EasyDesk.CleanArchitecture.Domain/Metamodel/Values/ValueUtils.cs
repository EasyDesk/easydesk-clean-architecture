namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

public static class ValueUtils
{
    public static T ToValue<T, TSelf>(PureValue<T, TSelf> wrapper) where T : IEquatable<T> where TSelf : PureValue<T, TSelf>, IValue<T> => wrapper.Value;
}
