using Microsoft.AspNetCore.Http;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

public interface ITenantReader
{
    Option<string> ReadFromHttpContext(HttpContext httpContext);

    Option<string> ReadFromMessageContext(IMessageContext messageContext);
}
