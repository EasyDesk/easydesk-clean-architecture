using Autofac;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using EasyDesk.CleanArchitecture.Testing.Integration.Http;
using EasyDesk.CleanArchitecture.Testing.Integration.Refactor;
using EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Session;
using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Multitenancy;

public static class MultitenancyFixtureExtensions
{
    public static TestFixtureConfigurer AddMultitenancy(this TestFixtureConfigurer configurer)
    {
        configurer.ContainerBuilder
            .RegisterType<TestTenantManager>()
            .AsSelf()
            .As<IHttpRequestConfigurator>()
            .InstancePerLifetimeScope();

        configurer.ContainerBuilder
            .Register(_ => new DefaultTenantInfo(None))
            .InstancePerLifetimeScope();

        return configurer;
    }

    public static SessionConfigurer SetDefaultTenant(this SessionConfigurer configurer, TenantId tenantId) =>
        configurer.SetDefaultTenant(TenantInfo.Tenant(tenantId));

    public static SessionConfigurer SetDefaultTenant(this SessionConfigurer configurer, TenantInfo tenantInfo) =>
        configurer.SetDefaultTenant(Some(tenantInfo));

    public static SessionConfigurer SetPublicTenant(this SessionConfigurer configurer) =>
        configurer.SetDefaultTenant(Some(TenantInfo.Public));

    public static SessionConfigurer IgnoreTenant(this SessionConfigurer configurer) =>
        configurer.SetDefaultTenant(None);

    public static SessionConfigurer SetDefaultTenant(this SessionConfigurer configurer, Option<TenantInfo> tenantInfo)
    {
        configurer.ContainerBuilder
            .Register(_ => new DefaultTenantInfo(tenantInfo))
            .InstancePerLifetimeScope();

        return configurer;
    }
}
