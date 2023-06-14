namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public abstract record ContextInfo
{
    public abstract record Request : ContextInfo;

    public record AuthenticatedRequest(Identity Identity) : Request;

    public record AnonymousRequest : Request;

    public record AsyncMessage : ContextInfo;

    public record Unknown : ContextInfo;
}
