namespace EasyDesk.CleanArchitecture.Application.ContextProvider;

public abstract record Context;

public abstract record RequestContext : Context;

public record AuthenticatedRequestContext(UserInfo UserInfo) : RequestContext;

public record AnonymousRequestContext : RequestContext;

public record AsyncMessageContext : Context;

public record NoContext : Context;
