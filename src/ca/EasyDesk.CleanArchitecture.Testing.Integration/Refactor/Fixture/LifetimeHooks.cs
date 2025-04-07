namespace EasyDesk.CleanArchitecture.Testing.Integration.Refactor.Fixture;

public abstract class LifetimeHooks : IFixtureLifetime
{
    public virtual Task OnInitialization() => Task.CompletedTask;

    public virtual Task BeforeTest() => Task.CompletedTask;

    public virtual Task AfterTest() => Task.CompletedTask;

    public virtual Task BetweenTests() => Task.CompletedTask;

    public virtual Task OnDisposal() => Task.CompletedTask;
}
