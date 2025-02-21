using EasyDesk.Commons.Options;

namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public interface IContextTenantDetector
{
    Option<string> TenantId { get; }
}
