using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using EasyDesk.Commons.Observables;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;

public class WebServiceTestsFixtureBuilder
{
    private readonly TestWebServiceBuilder _webServiceBuilder;
    private readonly ContainersCollection _containers;
    private readonly IAsyncObservable<ITestWebService> _onInitialization;
    private readonly IAsyncObservable<ITestWebService> _onReset;
    private readonly IAsyncObservable<ITestWebService> _onDisposal;

    public WebServiceTestsFixtureBuilder(
        TestWebServiceBuilder webServiceBuilder,
        ContainersCollection containers,
        IAsyncObservable<ITestWebService> onInitialization,
        IAsyncObservable<ITestWebService> onReset,
        IAsyncObservable<ITestWebService> onDisposal)
    {
        _webServiceBuilder = webServiceBuilder;
        _containers = containers;
        _onInitialization = onInitialization;
        _onReset = onReset;
        _onDisposal = onDisposal;
    }

    public WebServiceTestsFixtureBuilder ConfigureWebService(Action<TestWebServiceBuilder> configure)
    {
        configure(_webServiceBuilder);
        return this;
    }

    public WebServiceTestsFixtureBuilder ConfigureContainers(Action<ContainersCollection> configure)
    {
        configure(_containers);
        return this;
    }

    public WebServiceTestsFixtureBuilder OnInitialization(AsyncAction<ITestWebService> action) =>
        ConfigureLifecycleHook(_onInitialization, action);

    public WebServiceTestsFixtureBuilder OnReset(AsyncAction<ITestWebService> action) =>
        ConfigureLifecycleHook(_onReset, action);

    public WebServiceTestsFixtureBuilder OnDisposal(AsyncAction<ITestWebService> action) =>
        ConfigureLifecycleHook(_onDisposal, action);

    public WebServiceTestsFixtureBuilder OnInitialization(Action<ITestWebService> action) =>
        ConfigureLifecycleHook(_onInitialization, action);

    public WebServiceTestsFixtureBuilder OnReset(Action<ITestWebService> action) =>
        ConfigureLifecycleHook(_onReset, action);

    public WebServiceTestsFixtureBuilder OnDisposal(Action<ITestWebService> action) =>
        ConfigureLifecycleHook(_onDisposal, action);

    private WebServiceTestsFixtureBuilder ConfigureLifecycleHook(IAsyncObservable<ITestWebService> observable, AsyncAction<ITestWebService> action)
    {
        observable.Subscribe(action);
        return this;
    }

    private WebServiceTestsFixtureBuilder ConfigureLifecycleHook(IAsyncObservable<ITestWebService> observable, Action<ITestWebService> action)
    {
        observable.Subscribe(action);
        return this;
    }
}
