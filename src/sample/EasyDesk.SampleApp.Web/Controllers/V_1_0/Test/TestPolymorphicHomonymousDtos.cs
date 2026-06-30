namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Test
{
    public interface ITestHomonymousPolymorphicInterface
    {
        string AProperty { get; }
    }
}

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Test.TestHomonymousRecord1
{
    public record TestHomonymousRecord(string AProperty, string BProperty) : ITestHomonymousPolymorphicInterface;
}

namespace EasyDesk.SampleApp.Web.Controllers.V_1_0.Test.TestHomonymousRecord2
{
    public record TestHomonymousRecord(string AProperty, string CProperty) : ITestHomonymousPolymorphicInterface;
}
