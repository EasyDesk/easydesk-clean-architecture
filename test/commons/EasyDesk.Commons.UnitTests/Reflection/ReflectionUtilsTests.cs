using EasyDesk.Commons.Reflection;
using Shouldly;
using Xunit;

namespace EasyDesk.Commons.UnitTests.Reflection;

public class ReflectionUtilsTests
{
    private interface IInterface
    {
    }

    private interface IGenericInterface<T>
    {
    }

    private record NoInterface;

    private record Implementation : IInterface;

    private record OpenGenericImplementation<T> : IGenericInterface<T>;

    private record ClosedGenericImplementation : IGenericInterface<int>;

    private record MultiImplementation : IInterface, IGenericInterface<int>;

    private record BaseImplementation<T> : IGenericInterface<T>;

    private record ConcreteImplementation<T> : BaseImplementation<T>;

    [Theory]
    [MemberData(nameof(TestData))]
    public void TestIsSubtypeOrImplementationOf(Type type, Type otherType, bool expected)
    {
        type.IsSubtypeOrImplementationOf(otherType).ShouldBe(expected);
    }

    public static IEnumerable<object[]> TestData()
    {
        yield return new object[] { typeof(NoInterface), typeof(IInterface), false };
        yield return new object[] { typeof(NoInterface), typeof(IGenericInterface<>), false };
        yield return new object[] { typeof(NoInterface), typeof(IGenericInterface<int>), false };
        yield return new object[] { typeof(Implementation), typeof(IInterface), true };
        yield return new object[] { typeof(Implementation), typeof(IGenericInterface<>), false };
        yield return new object[] { typeof(Implementation), typeof(IGenericInterface<int>), false };
        yield return new object[] { typeof(OpenGenericImplementation<>), typeof(IGenericInterface<>), true };
        yield return new object[] { typeof(OpenGenericImplementation<>), typeof(IGenericInterface<int>), true };
        yield return new object[] { typeof(OpenGenericImplementation<int>), typeof(IGenericInterface<>), true };
        yield return new object[] { typeof(OpenGenericImplementation<int>), typeof(IGenericInterface<int>), true };
        yield return new object[] { typeof(OpenGenericImplementation<int>), typeof(IGenericInterface<string>), false };
        yield return new object[] { typeof(ClosedGenericImplementation), typeof(IGenericInterface<>), true };
        yield return new object[] { typeof(ClosedGenericImplementation), typeof(IGenericInterface<int>), true };
        yield return new object[] { typeof(ClosedGenericImplementation), typeof(IGenericInterface<string>), false };
        yield return new object[] { typeof(MultiImplementation), typeof(IInterface), true };
        yield return new object[] { typeof(MultiImplementation), typeof(IGenericInterface<>), true };
        yield return new object[] { typeof(MultiImplementation), typeof(IGenericInterface<int>), true };
        yield return new object[] { typeof(MultiImplementation), typeof(IGenericInterface<string>), false };
        yield return new object[] { typeof(ConcreteImplementation<>), typeof(IGenericInterface<>), true };
        yield return new object[] { typeof(ConcreteImplementation<>), typeof(IGenericInterface<int>), true };
        yield return new object[] { typeof(ConcreteImplementation<>), typeof(BaseImplementation<>), true };
        yield return new object[] { typeof(ConcreteImplementation<>), typeof(BaseImplementation<int>), true };
        yield return new object[] { typeof(ConcreteImplementation<int>), typeof(IGenericInterface<>), true };
        yield return new object[] { typeof(ConcreteImplementation<int>), typeof(IGenericInterface<int>), true };
        yield return new object[] { typeof(ConcreteImplementation<int>), typeof(IGenericInterface<string>), false };
        yield return new object[] { typeof(ConcreteImplementation<int>), typeof(BaseImplementation<>), true };
        yield return new object[] { typeof(ConcreteImplementation<int>), typeof(BaseImplementation<int>), true };
        yield return new object[] { typeof(ConcreteImplementation<int>), typeof(BaseImplementation<string>), false };
    }
}
