using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing.Model;

internal class AuditIdentityModel
{
    public long AuditRecordId { get; set; }

    public required string IdentityRealm { get; set; }

    public required string Identity { get; set; }

    public ICollection<AuditIdentityAttributeModel> IdentityAttributes { get; set; } = new HashSet<AuditIdentityAttributeModel>();

    public Identity ToIdentity() => new(new Realm(IdentityRealm), new IdentityId(Identity), CreateAttributeCollection());

    private AttributeCollection CreateAttributeCollection() =>
        AttributeCollection.FromFlatKeyValuePairs(IdentityAttributes.Select(a => (a.Key, a.Value)));

    public static AuditIdentityModel FromIdentity(Realm realm, Identity identity)
    {
        var model = new AuditIdentityModel
        {
            Identity = identity.Id,
            IdentityRealm = realm,
        };

        identity
            .Attributes
            .AttributeMap
            .SelectMany(a => a.Value.Select(v => new AuditIdentityAttributeModel
            {
                Realm = realm,
                Key = a.Key,
                Value = v,
            }))
            .AddTo(model.IdentityAttributes);

        return model;
    }
}
