namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public class BasicContextResetter : IContextResetter
{
    public Task ResetContext() => Task.CompletedTask;
}
