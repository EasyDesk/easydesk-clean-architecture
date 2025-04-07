using Autofac;
using EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Fixture;
using EasyDesk.Commons.Tasks;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor;

public static class TestFixtureLifetimeExtensions
{
    public static TestFixtureConfigurer RegisterLifetimeHooks<T>(this TestFixtureConfigurer configurer)
        where T : IFixtureLifetime
    {
        configurer.ContainerBuilder.RegisterType<T>()
            .As<IFixtureLifetime>()
            .SingleInstance();

        return configurer;
    }

    public static TestFixtureConfigurer RegisterLifetimeHooks<T>(this TestFixtureConfigurer configurer, Func<IComponentContext, T> factory)
        where T : IFixtureLifetime
    {
        configurer.ContainerBuilder.Register(factory)
            .As<IFixtureLifetime>()
            .SingleInstance();

        return configurer;
    }

    public static TestFixtureConfigurer RegisterLifetimeHooks(
        this TestFixtureConfigurer configurer,
        AsyncAction? onInitialization = null,
        AsyncAction? beforeTest = null,
        AsyncAction? afterTest = null,
        AsyncAction? betweenTests = null,
        AsyncAction? onDisposal = null)
    {
        configurer.ContainerBuilder
            .RegisterInstance(new FixtureLifetimeHooks(
                onInitialization: onInitialization,
                beforeTest: beforeTest,
                afterTest: afterTest,
                betweenTests: betweenTests,
                onDisposal: onDisposal))
            .As<IFixtureLifetime>()
            .SingleInstance();

        return configurer;
    }

    public static TestFixtureConfigurer RegisterLifetimeHooks(
        this TestFixtureConfigurer configurer,
        Action? onInitialization = null,
        Action? beforeTest = null,
        Action? afterTest = null,
        Action? betweenTests = null,
        Action? onDisposal = null)
    {
        return configurer.RegisterLifetimeHooks(
            onInitialization: AsAsyncAction(onInitialization),
            beforeTest: AsAsyncAction(beforeTest),
            afterTest: AsAsyncAction(afterTest),
            betweenTests: AsAsyncAction(betweenTests),
            onDisposal: AsAsyncAction(onDisposal));
    }

    private static AsyncAction? AsAsyncAction(Action? action)
    {
        if (action is null)
        {
            return null;
        }

        return () =>
        {
            action();
            return Task.CompletedTask;
        };
    }

    internal class FixtureLifetimeHooks : IFixtureLifetime
    {
        private readonly AsyncAction? _onInitialization;
        private readonly AsyncAction? _beforeTest;
        private readonly AsyncAction? _afterTest;
        private readonly AsyncAction? _betweenTests;
        private readonly AsyncAction? _onDisposal;

        public FixtureLifetimeHooks(
            AsyncAction? onInitialization,
            AsyncAction? beforeTest,
            AsyncAction? afterTest,
            AsyncAction? betweenTests,
            AsyncAction? onDisposal)
        {
            _onInitialization = onInitialization;
            _beforeTest = beforeTest;
            _afterTest = afterTest;
            _betweenTests = betweenTests;
            _onDisposal = onDisposal;
        }

        public Task OnInitialization() => _onInitialization?.Invoke() ?? Task.CompletedTask;

        public Task BeforeTest() => _beforeTest?.Invoke() ?? Task.CompletedTask;

        public Task AfterTest() => _afterTest?.Invoke() ?? Task.CompletedTask;

        public Task BetweenTests() => _betweenTests?.Invoke() ?? Task.CompletedTask;

        public Task OnDisposal() => _onDisposal?.Invoke() ?? Task.CompletedTask;
    }
}
