namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auth.Model;

internal class ApiKeyIdentityAttributeModel
{
    public long Id { get; set; }

    public long IdentityId { get; set; }

    public required string AttributeName { get; set; }

    public required string AttributeValue { get; set; }
}
