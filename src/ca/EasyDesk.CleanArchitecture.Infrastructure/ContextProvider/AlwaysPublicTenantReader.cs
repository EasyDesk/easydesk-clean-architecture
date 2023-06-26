using Microsoft.AspNetCore.Http;
using Rebus.Pipeline;

namespace EasyDesk.CleanArchitecture.Infrastructure.ContextProvider;

internal class AlwaysPublicTenantReader : ITenantReader
{
    public Option<string> ReadFromHttpContext(HttpContext httpContext) => None;

    public Option<string> ReadFromMessageContext(IMessageContext messageContext) => None;
}
