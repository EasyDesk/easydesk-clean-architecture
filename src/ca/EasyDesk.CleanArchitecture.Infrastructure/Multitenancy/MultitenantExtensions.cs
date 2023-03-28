using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.CleanArchitecture.Application.Multitenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDesk.CleanArchitecture.Infrastructure.Multitenancy;

public static class MultitenantExtensions
{
    public static void UseDefaultContextTenantReader(this MultitenancyOptions options)
    {
        options.TenantReaderImplementation = p => new DefaultContextTenantReader(
            p.GetRequiredService<IContextProvider>(),
            p.GetRequiredService<IHttpContextAccessor>());
    }
}
