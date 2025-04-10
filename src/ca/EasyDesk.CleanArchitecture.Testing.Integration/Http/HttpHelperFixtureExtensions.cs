﻿using Autofac;
using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.CleanArchitecture.Application.Json;
using EasyDesk.CleanArchitecture.Testing.Integration.Fixture;
using EasyDesk.CleanArchitecture.Testing.Integration.Host;
using EasyDesk.CleanArchitecture.Testing.Integration.Http.Builders.Base;
using EasyDesk.CleanArchitecture.Testing.Integration.Session;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Http;

public static class HttpHelperFixtureExtensions
{
    public static TestFixtureConfigurer AddHttpTestHelper(this TestFixtureConfigurer configurer)
    {
        configurer.ContainerBuilder
            .Register(c => TestHttpAuthentication.CreateFromServices(c.Resolve<ITestHost>().LifetimeScope))
            .SingleInstance();

        configurer.ContainerBuilder
            .Register(c =>
            {
                var jsonSettings = c.Resolve<ITestHost>().LifetimeScope.Resolve<JsonOptionsConfigurator>();
                return new HttpTestHelper(c.Resolve<HttpClient>(), jsonSettings, c.Resolve<IEnumerable<IHttpRequestConfigurator>>(), c.Resolve<ITestHttpAuthentication>());
            })
            .InstancePerLifetimeScope();

        configurer.ContainerBuilder
            .RegisterType<TestAuthenticationManager>()
            .AsSelf()
            .As<IHttpRequestConfigurator>()
            .InstancePerLifetimeScope();

        configurer.ContainerBuilder
            .Register(_ => new DefaultTestAgent(None))
            .InstancePerLifetimeScope();

        return configurer;
    }

    public static SessionConfigurer SetAnonymous(this SessionConfigurer configurer) =>
        configurer.SetDefaultAgent(None);

    public static SessionConfigurer SetDefaultAgent(this SessionConfigurer configurer, Agent agent) =>
        configurer.SetDefaultAgent(Some(agent));

    public static SessionConfigurer SetDefaultAgent(this SessionConfigurer configurer, Option<Agent> agent)
    {
        configurer.ContainerBuilder
            .Register(_ => new DefaultTestAgent(agent))
            .InstancePerLifetimeScope();

        return configurer;
    }

    public static SessionConfigurer ConfigureHttpRequests(this SessionConfigurer configurer, Action<HttpRequestBuilder> configureRequest)
    {
        configurer.ContainerBuilder
            .RegisterInstance(new HttpRequestConfigurator(configureRequest))
            .As<IHttpRequestConfigurator>()
            .SingleInstance();
        return configurer;
    }
}
