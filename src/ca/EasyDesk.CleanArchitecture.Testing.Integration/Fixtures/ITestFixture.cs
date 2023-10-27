using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using NodaTime.Testing;
using Xunit;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;

public interface ITestFixture : IAsyncLifetime
{
    FakeClock Clock { get; }

    ITestWebService WebService { get; }

    Task BeforeTest();

    Task AfterTest();

    Task Reset();
}
