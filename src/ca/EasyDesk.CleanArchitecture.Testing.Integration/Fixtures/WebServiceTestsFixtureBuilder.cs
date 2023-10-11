using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using EasyDesk.Commons.Observables;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;

public sealed class WebServiceTestsFixtureBuilder
{
    private readonly TestWebServiceBuilder _webServiceBuilder;
    private readonly ContainersCollection _containers;
    private readonly IAsyncObservable<ITestWebService> _onInitialization;
    private readonly IAsyncObservable<ITestWebService> _beforeEachTest;
    private readonly IAsyncObservable<ITestWebService> _afterEachTest;
    private readonly IAsyncObservable<ITestWebService> _onReset;
    private readonly IAsyncObservable<ITestWebService> _onDisposal;

    public WebServiceTestsFixtureBuilder(
        TestWebServiceBuilder webServiceBuilder,
        ContainersCollection containers,
        IAsyncObservable<ITestWebService> onInitialization,
        IAsyncObservable<ITestWebService> beforeEachTest,
        IAsyncObservable<ITestWebService> afterEachTest,
        IAsyncObservable<ITestWebService> onReset,
        IAsyncObservable<ITestWebService> onDisposal)
    {
        _webServiceBuilder = webServiceBuilder;
        _containers = containers;
        _onInitialization = onInitialization;
        _beforeEachTest = beforeEachTest;
        _afterEachTest = afterEachTest;
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

    public WebServiceTestsFixtureBuilder OnInitialization(Action<ITestWebService> action) =>
        ConfigureLifecycleHook(_onInitialization, action);

    public WebServiceTestsFixtureBuilder BeforeEachTest(AsyncAction<ITestWebService> action) =>
        ConfigureLifecycleHook(_beforeEachTest, action);

    public WebServiceTestsFixtureBuilder BeforeEachTest(Action<ITestWebService> action) =>
        ConfigureLifecycleHook(_beforeEachTest, action);

    public WebServiceTestsFixtureBuilder AfterEachTest(AsyncAction<ITestWebService> action) =>
        ConfigureLifecycleHook(_afterEachTest, action);

    public WebServiceTestsFixtureBuilder AfterEachTest(Action<ITestWebService> action) =>
        ConfigureLifecycleHook(_afterEachTest, action);

    public WebServiceTestsFixtureBuilder OnReset(AsyncAction<ITestWebService> action) =>
        ConfigureLifecycleHook(_onReset, action);

    public WebServiceTestsFixtureBuilder OnReset(Action<ITestWebService> action) =>
        ConfigureLifecycleHook(_onReset, action);

    public WebServiceTestsFixtureBuilder OnDisposal(AsyncAction<ITestWebService> action) =>
        ConfigureLifecycleHook(_onDisposal, action);

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
