using EasyDesk.Tools.Options;
using static EasyDesk.Tools.Options.OptionImports;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public class NoTenant : ITenantProvider
{
    public Option<string> TenantId => None;
}
