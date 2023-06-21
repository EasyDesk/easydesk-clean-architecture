namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing.Model;

internal class AuditIdentityAttributeModel
{
    public long AuditRecordId { get; set; }

    required public string Realm { get; set; }

    required public string Key { get; set; }

    required public string Value { get; set; }
}
