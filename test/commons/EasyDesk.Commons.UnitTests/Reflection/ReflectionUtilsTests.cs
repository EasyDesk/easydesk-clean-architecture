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

    public static TheoryData<Type, Type, bool> TestData() => new()
    {
        { typeof(NoInterface), typeof(IInterface), false },
        { typeof(NoInterface), typeof(IGenericInterface<>), false },
        { typeof(NoInterface), typeof(IGenericInterface<int>), false },
        { typeof(Implementation), typeof(IInterface), true },
        { typeof(Implementation), typeof(IGenericInterface<>), false },
        { typeof(Implementation), typeof(IGenericInterface<int>), false },
        { typeof(OpenGenericImplementation<>), typeof(IGenericInterface<>), true },
        { typeof(OpenGenericImplementation<>), typeof(IGenericInterface<int>), true },
        { typeof(OpenGenericImplementation<int>), typeof(IGenericInterface<>), true },
        { typeof(OpenGenericImplementation<int>), typeof(IGenericInterface<int>), true },
        { typeof(OpenGenericImplementation<int>), typeof(IGenericInterface<string>), false },
        { typeof(ClosedGenericImplementation), typeof(IGenericInterface<>), true },
        { typeof(ClosedGenericImplementation), typeof(IGenericInterface<int>), true },
        { typeof(ClosedGenericImplementation), typeof(IGenericInterface<string>), false },
        { typeof(MultiImplementation), typeof(IInterface), true },
        { typeof(MultiImplementation), typeof(IGenericInterface<>), true },
        { typeof(MultiImplementation), typeof(IGenericInterface<int>), true },
        { typeof(MultiImplementation), typeof(IGenericInterface<string>), false },
        { typeof(ConcreteImplementation<>), typeof(IGenericInterface<>), true },
        { typeof(ConcreteImplementation<>), typeof(IGenericInterface<int>), true },
        { typeof(ConcreteImplementation<>), typeof(BaseImplementation<>), true },
        { typeof(ConcreteImplementation<>), typeof(BaseImplementation<int>), true },
        { typeof(ConcreteImplementation<int>), typeof(IGenericInterface<>), true },
        { typeof(ConcreteImplementation<int>), typeof(IGenericInterface<int>), true },
        { typeof(ConcreteImplementation<int>), typeof(IGenericInterface<string>), false },
        { typeof(ConcreteImplementation<int>), typeof(BaseImplementation<>), true },
        { typeof(ConcreteImplementation<int>), typeof(BaseImplementation<int>), true },
        { typeof(ConcreteImplementation<int>), typeof(BaseImplementation<string>), false },
    };
}
