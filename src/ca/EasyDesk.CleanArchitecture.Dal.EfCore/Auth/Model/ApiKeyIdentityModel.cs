using EasyDesk.CleanArchitecture.Application.Authentication;
using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auth.Model;

internal class ApiKeyIdentityModel
{
    public long Id { get; set; }

    public required string IdentityId { get; set; }

    public required string IdentityRealm { get; set; }

    public long ApiKeyId { get; set; }

    public ICollection<ApiKeyIdentityAttributeModel> Attributes { get; set; } = [];

    public Identity ToIdentity()
    {
        return Identity.Create(new(IdentityRealm), new(IdentityId), Attributes.Select(x => (x.AttributeName, x.AttributeValue)));
    }

    public static ApiKeyIdentityModel FromIdentity(Identity identity)
    {
        var model = new ApiKeyIdentityModel
        {
            IdentityId = identity.Id,
            IdentityRealm = identity.Realm,
        };

        var attributes =
            from attribute in identity.Attributes.AttributeMap
            from value in attribute.Value
            select new ApiKeyIdentityAttributeModel
            {
                AttributeName = attribute.Key,
                AttributeValue = value,
            };

        attributes.AddTo(model.Attributes);

        return model;
    }
}
