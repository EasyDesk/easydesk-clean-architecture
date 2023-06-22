namespace EasyDesk.CleanArchitecture.Dal.EfCore.Auditing.Model;

internal class AuditIdentityAttributeModel
{
    public long AuditRecordId { get; set; }

    public required string Realm { get; set; }

    public required string Key { get; set; }

    public required string Value { get; set; }
}
