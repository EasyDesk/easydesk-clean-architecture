namespace EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;

public interface IMultitenantEntity
{
    public string? TenantId { get; set; }
}
