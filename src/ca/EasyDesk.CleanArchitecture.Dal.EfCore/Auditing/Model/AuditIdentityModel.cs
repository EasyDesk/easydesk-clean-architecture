using EasyDesk.CleanArchitecture.Application.ContextProvider;
using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing.Model;

internal class AuditIdentityModel
{
    public long AuditRecordId { get; set; }

    required public string Name { get; set; }

    required public string Identity { get; set; }

    public ICollection<AuditIdentityAttributeModel> IdentityAttributes { get; set; } = new HashSet<AuditIdentityAttributeModel>();

    public Identity ToIdentity() => new(IdentityId.New(Identity), CreateAttributeCollection());

    private AttributeCollection CreateAttributeCollection() =>
        AttributeCollection.FromFlatKeyValuePairs(IdentityAttributes.Select(a => (a.Key, a.Value)));

    public static AuditIdentityModel FromIdentity(string name, Identity identity)
    {
        var model = new AuditIdentityModel
        {
            Identity = identity.Id,
            Name = name,
        };

        identity
            .Attributes
            .Attributes
            .SelectMany(a => a.Value.Select(v => new AuditIdentityAttributeModel
            {
                Name = name,
                Key = a.Key,
                Value = v,
            }))
            .AddTo(model.IdentityAttributes);

        return model;
    }
}
