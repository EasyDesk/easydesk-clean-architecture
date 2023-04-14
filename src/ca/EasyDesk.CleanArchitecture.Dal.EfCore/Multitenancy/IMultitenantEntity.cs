namespace EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy;

public interface IMultitenantEntity
{
    string? Tenant { get; set; }
}
