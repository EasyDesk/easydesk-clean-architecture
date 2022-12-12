namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

public interface IContextTenantReader
{
    Option<string> GetTenantId();
}
