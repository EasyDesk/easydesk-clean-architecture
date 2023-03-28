namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public interface IContextProvider : IUserInfoProvider
{
    Context Context { get; }
}
