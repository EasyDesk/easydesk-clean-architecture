namespace EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;

public interface IMultitenantEntity
{
    string? TenantId { get; set; }
}
