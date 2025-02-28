using Autofac;
using Autofac.Core;
using EasyDesk.CleanArchitecture.DependencyInjection;
using Shouldly;

namespace EasyDesk.CleanArchitecture.UnitTests.DependencyInjection;

public sealed class UseCaseScopeTests : IDisposable
{
    private readonly ILifetimeScope _rootScope;

    private readonly ILifetimeScope _scopeUnderRoot;

    private readonly ILifetimeScope _useCaseScopeUnderScopeUnderRoot;

    private readonly ILifetimeScope _scopeUnderUseCase;

    public void Dispose()
    {
        _scopeUnderUseCase.Dispose();
        _useCaseScopeUnderScopeUnderRoot.Dispose();
        _scopeUnderRoot.Dispose();
        _rootScope.Dispose();
    }

    public record InstancePerUseCase;

    public record InstancePerUseCaseOrRoot;

    public record InstancePerUseCaseOrSelf;

    public record InstancePerUseCaseExtended;

    public UseCaseScopeTests()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<InstancePerUseCase>().InstancePerMatchingLifetimeScope(LifetimeScopes.UseCaseScopeTag);
        builder.RegisterType<InstancePerUseCaseOrRoot>().InstancePerMatchingOrRootLifetimeScope(LifetimeScopes.UseCaseScopeTag);
        builder.RegisterType<InstancePerUseCaseOrSelf>().InstancePerMatchingOrMostNestedLifetimeScope(LifetimeScopes.UseCaseScopeTag);
        builder.RegisterType<InstancePerUseCaseExtended>().InstancePerUseCase();

        _rootScope = builder.Build();
        _scopeUnderRoot = _rootScope.BeginLifetimeScope();
        _useCaseScopeUnderScopeUnderRoot = _scopeUnderRoot.BeginUseCaseLifetimeScope();
        _scopeUnderUseCase = _useCaseScopeUnderScopeUnderRoot.BeginLifetimeScope();
    }

    [Fact]
    public void InstancePerUseCase_ShouldNotResolveOnlyOutsideUseCase()
    {
        Should.Throw<DependencyResolutionException>(_rootScope.Resolve<InstancePerUseCase>);
        Should.Throw<DependencyResolutionException>(_scopeUnderRoot.Resolve<InstancePerUseCase>);
        Should.NotThrow(_useCaseScopeUnderScopeUnderRoot.Resolve<InstancePerUseCase>);
        Should.NotThrow(_scopeUnderUseCase.Resolve<InstancePerUseCase>);
    }

    [Fact]
    public void InstancePerUseCase_ShouldShareInstanceUnderUseCase()
    {
        var x = _useCaseScopeUnderScopeUnderRoot.Resolve<InstancePerUseCase>();
        var y = _useCaseScopeUnderScopeUnderRoot.Resolve<InstancePerUseCase>();
        var z = _scopeUnderUseCase.Resolve<InstancePerUseCase>();
        x.ShouldBeSameAs(y);
        x.ShouldBeSameAs(z);
    }

    [Fact]
    public void InstancePerUseCaseOrRoot_ShouldResolveAlways()
    {
        Should.NotThrow(_rootScope.Resolve<InstancePerUseCaseOrRoot>);
        Should.NotThrow(_scopeUnderRoot.Resolve<InstancePerUseCaseOrRoot>);
        Should.NotThrow(_useCaseScopeUnderScopeUnderRoot.Resolve<InstancePerUseCaseOrRoot>);
        Should.NotThrow(_scopeUnderUseCase.Resolve<InstancePerUseCaseOrRoot>);
    }

    [Fact]
    public void InstancePerUseCaseOrRoot_ShouldShareInstanceUnderUseCase()
    {
        var x = _useCaseScopeUnderScopeUnderRoot.Resolve<InstancePerUseCaseOrRoot>();
        var y = _useCaseScopeUnderScopeUnderRoot.Resolve<InstancePerUseCaseOrRoot>();
        var z = _scopeUnderUseCase.Resolve<InstancePerUseCaseOrRoot>();
        x.ShouldBeSameAs(y);
        x.ShouldBeSameAs(z);
    }

    [Fact]
    public void InstancePerUseCaseOrRoot_ShouldShareInstanceUnderRoot()
    {
        var x = _rootScope.Resolve<InstancePerUseCaseOrRoot>();
        var y = _rootScope.Resolve<InstancePerUseCaseOrRoot>();
        var z = _scopeUnderRoot.Resolve<InstancePerUseCaseOrRoot>();
        x.ShouldBeSameAs(y);
        x.ShouldBeSameAs(z);
    }

    [Fact]
    public void InstancePerUseCaseOrRoot_ShouldNotShareInstanceBetweenUseCaseAndRoot()
    {
        var x = _rootScope.Resolve<InstancePerUseCaseOrRoot>();
        var y = _scopeUnderRoot.Resolve<InstancePerUseCaseOrRoot>();
        var z = _useCaseScopeUnderScopeUnderRoot.Resolve<InstancePerUseCaseOrRoot>();
        z.ShouldNotBeSameAs(x);
        z.ShouldNotBeSameAs(y);
    }

    [Fact]
    public void InstancePerUseCaseOrSelf_ShouldAlwaysResolve()
    {
        Should.NotThrow(_rootScope.Resolve<InstancePerUseCaseOrSelf>);
        Should.NotThrow(_scopeUnderRoot.Resolve<InstancePerUseCaseOrSelf>);
        Should.NotThrow(_useCaseScopeUnderScopeUnderRoot.Resolve<InstancePerUseCaseOrSelf>);
        Should.NotThrow(_scopeUnderUseCase.Resolve<InstancePerUseCaseOrSelf>);
    }

    [Fact]
    public void InstancePerUseCaseOrSelf_ShouldShareInstanceUnderUseCase()
    {
        var x = _useCaseScopeUnderScopeUnderRoot.Resolve<InstancePerUseCaseOrSelf>();
        var y = _useCaseScopeUnderScopeUnderRoot.Resolve<InstancePerUseCaseOrSelf>();
        var z = _scopeUnderUseCase.Resolve<InstancePerUseCaseOrSelf>();
        x.ShouldBeSameAs(y);
        x.ShouldBeSameAs(z);
    }

    [Fact]
    public void InstancePerUseCaseOrSelf_ShouldShareInstanceUnderScoped()
    {
        var x = _scopeUnderRoot.Resolve<InstancePerUseCaseOrSelf>();
        var y = _scopeUnderRoot.Resolve<InstancePerUseCaseOrSelf>();
        x.ShouldBeSameAs(y);
    }

    [Fact]
    public void InstancePerUseCaseOrSelf_ShouldNotShareInstanceBetweenUseCaseAndScope()
    {
        var x = _scopeUnderRoot.Resolve<InstancePerUseCaseOrSelf>();
        var y = _useCaseScopeUnderScopeUnderRoot.Resolve<InstancePerUseCaseOrSelf>();
        y.ShouldNotBeSameAs(x);
    }

    [Fact]
    public void InstancePerUseCaseOrSelf_ShouldNotShareInstanceBetweenRootAndScope()
    {
        var x = _scopeUnderRoot.Resolve<InstancePerUseCaseOrSelf>();
        var y = _rootScope.Resolve<InstancePerUseCaseOrSelf>();
        y.ShouldNotBeSameAs(x);
    }
}
