namespace EasyDesk.CleanArchitecture.Application.Multitenancy;

internal class PublicContextTenantReader : IContextTenantReader
{
    public Option<string> GetTenantId() => None;
}
