namespace EasyDesk.CleanArchitecture.Domain.Metamodel.Values;

public static class ValueWrapperUtils
{
    public static T ToValue<T, W>(AbstractValueWrapper<T, W> wrapper) where T : IEquatable<T> where W : AbstractValueWrapper<T, W> => wrapper.Value;
}
