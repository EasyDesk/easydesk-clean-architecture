namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

public static class ValueWrapperUtils
{
    public static T ToValue<T>(AbstractValueWrapper<T> wrapper) where T : IEquatable<T> => wrapper.Value;
}
