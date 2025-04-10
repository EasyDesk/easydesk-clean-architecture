using Autofac;
using EasyDesk.CleanArchitecture.Infrastructure.BackgroundTasks;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using EasyDesk.Commons.Collections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Host;

public static class HostFixtureExtensions
{
    public const string DefaultEnvironment = "IntegrationTest";

    public static TestFixtureConfigurer RegisterHost<T>(this TestFixtureConfigurer configurer)
        where T : class
    {
        return configurer.RegisterHostInternal<T>(x => x.Configure(_ => { }));
    }

    public static TestFixtureConfigurer RegisterWebHost<T>(this TestFixtureConfigurer configurer)
        where T : class
    {
        configurer.ContainerBuilder
            .Register(c => c.Resolve<TestHostApplicationFactory<T>>().CreateClient())
            .InstancePerLifetimeScope();

        return configurer.RegisterHostInternal<T>();
    }

    private static TestFixtureConfigurer RegisterHostInternal<T>(this TestFixtureConfigurer configurer, Action<IWebHostBuilder>? configureWebHost = null)
        where T : class
    {
        configurer.ContainerBuilder
            .Register(c =>
            {
                var configurationActions = c.Resolve<IEnumerable<Action<ITestHostBuilder>>>();
                return new TestHostBuilder<T>(configureWebHost)
                    .Also(b =>
                    {
                        b.WithEnvironment(DefaultEnvironment);
                        configurationActions.ForEach(a => a(b));
                    })
                    .CreateFactory();
            })
            .SingleInstance();

        configurer.ContainerBuilder
            .RegisterType<TestHost<T>>()
            .As<ITestHost>()
            .SingleInstance();

        configurer.ContainerBuilder
            .Register(c => c.Resolve<TestHostApplicationFactory<T>>().CreateClient())
            .InstancePerDependency();

        configurer.ContainerBuilder
            .RegisterType<HostPausable>()
            .As<IPausable>()
            .SingleInstance();

        configurer.RegisterLifetimeHooks<HostLifetimeHooks>();

        return configurer;
    }

    public static TestFixtureConfigurer ConfigureHost(this TestFixtureConfigurer configurer, Action<ITestHostBuilder> configureHost)
    {
        configurer.ContainerBuilder
            .RegisterInstance(configureHost)
            .SingleInstance();

        return configurer;
    }

    private class HostLifetimeHooks : LifetimeHooks
    {
        private readonly ITestHost _host;

        public HostLifetimeHooks(ITestHost host)
        {
            _host = host;
        }

        public override Task OnInitialization() => _host.Start();
    }

    private class HostPausable : IPausable
    {
        private readonly ITestHost _host;

        public HostPausable(ITestHost host)
        {
            _host = host;
        }

        public async Task Pause(CancellationToken cancellationToken)
        {
            foreach (var hostedService in PausableHostedServices())
            {
                await hostedService.Pause(CancellationToken.None);
            }
        }

        public async Task Resume(CancellationToken cancellationToken)
        {
            foreach (var hostedService in PausableHostedServices())
            {
                await hostedService.Resume(CancellationToken.None);
            }
        }

        private IEnumerable<IPausableHostedService> PausableHostedServices() => _host.LifetimeScope
            .Resolve<IEnumerable<IHostedService>>()
            .SelectMany(h => h is IPausableHostedService p ? Some(p) : None);
    }
}
