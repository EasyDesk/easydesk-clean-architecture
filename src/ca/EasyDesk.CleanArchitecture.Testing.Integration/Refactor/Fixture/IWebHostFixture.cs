namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Fixture;

public interface IWebHostFixture : IHostFixture
{
    new ITestWebHost Host { get; }
}
