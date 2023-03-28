namespace EasyDesk.CleanArchitecture.Testing.Integration.Web;

public interface ITestWebService : IAsyncDisposable
{
    HttpClient HttpClient { get; }

    IServiceProvider Services { get; }
}
