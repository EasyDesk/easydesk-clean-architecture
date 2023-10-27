using EasyDesk.CleanArchitecture.Testing.Integration.Containers;
using EasyDesk.CleanArchitecture.Testing.Integration.Web;
using EasyDesk.Commons.Observables;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Fixtures;

public sealed class WebServiceTestsFixtureBuilder<T>
    where T : ITestFixture
{
    private readonly TestWebServiceBuilder _webServiceBuilder;
    private readonly ContainersCollection _containers;
    private readonly IAsyncObservable<T> _onInitialization;
    private readonly IAsyncObservable<T> _beforeEachTest;
    private readonly IAsyncObservable<T> _afterEachTest;
    private readonly IAsyncObservable<T> _onReset;
    private readonly IAsyncObservable<T> _onDisposal;

    public WebServiceTestsFixtureBuilder(
        TestWebServiceBuilder webServiceBuilder,
        ContainersCollection containers,
        IAsyncObservable<T> onInitialization,
        IAsyncObservable<T> beforeEachTest,
        IAsyncObservable<T> afterEachTest,
        IAsyncObservable<T> onReset,
        IAsyncObservable<T> onDisposal)
    {
        _webServiceBuilder = webServiceBuilder;
        _containers = containers;
        _onInitialization = onInitialization;
        _beforeEachTest = beforeEachTest;
        _afterEachTest = afterEachTest;
        _onReset = onReset;
        _onDisposal = onDisposal;
    }

    public WebServiceTestsFixtureBuilder<T> ConfigureWebService(Action<TestWebServiceBuilder> configure)
    {
        configure(_webServiceBuilder);
        return this;
    }

    public WebServiceTestsFixtureBuilder<T> ConfigureContainers(Action<ContainersCollection> configure)
    {
        configure(_containers);
        return this;
    }

    public WebServiceTestsFixtureBuilder<T> OnInitialization(AsyncAction<T> action) =>
        ConfigureLifecycleHook(_onInitialization, action);

    public WebServiceTestsFixtureBuilder<T> OnInitialization(Action<T> action) =>
        ConfigureLifecycleHook(_onInitialization, action);

    public WebServiceTestsFixtureBuilder<T> BeforeEachTest(AsyncAction<T> action) =>
        ConfigureLifecycleHook(_beforeEachTest, action);

    public WebServiceTestsFixtureBuilder<T> BeforeEachTest(Action<T> action) =>
        ConfigureLifecycleHook(_beforeEachTest, action);

    public WebServiceTestsFixtureBuilder<T> AfterEachTest(AsyncAction<T> action) =>
        ConfigureLifecycleHook(_afterEachTest, action);

    public WebServiceTestsFixtureBuilder<T> AfterEachTest(Action<T> action) =>
        ConfigureLifecycleHook(_afterEachTest, action);

    public WebServiceTestsFixtureBuilder<T> OnReset(AsyncAction<T> action) =>
        ConfigureLifecycleHook(_onReset, action);

    public WebServiceTestsFixtureBuilder<T> OnReset(Action<T> action) =>
        ConfigureLifecycleHook(_onReset, action);

    public WebServiceTestsFixtureBuilder<T> OnDisposal(AsyncAction<T> action) =>
        ConfigureLifecycleHook(_onDisposal, action);

    public WebServiceTestsFixtureBuilder<T> OnDisposal(Action<T> action) =>
        ConfigureLifecycleHook(_onDisposal, action);

    private WebServiceTestsFixtureBuilder<T> ConfigureLifecycleHook(IAsyncObservable<T> observable, AsyncAction<T> action)
    {
        observable.Subscribe(action);
        return this;
    }

    private WebServiceTestsFixtureBuilder<T> ConfigureLifecycleHook(IAsyncObservable<T> observable, Action<T> action)
    {
        observable.Subscribe(action);
        return this;
    }
}
