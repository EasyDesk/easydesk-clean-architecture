namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing.Model;

internal class AuditIdentityAttributeModel
{
    public long Id { get; set; }

    public long AuditRecordId { get; set; }

    required public string Key { get; set; }

    required public string Value { get; set; }
}
