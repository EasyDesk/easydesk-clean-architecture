namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor;

public interface ITestWebHost : ITestHost
{
    HttpClient CreateHttpClient();
}
