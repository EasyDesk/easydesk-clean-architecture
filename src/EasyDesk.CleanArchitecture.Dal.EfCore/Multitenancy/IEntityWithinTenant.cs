namespace EasyDesk.CleanArchitecture.Dal.EfCore.Multitenancy
{
    public interface IEntityWithinTenant
    {
        public string TenantId { get; set; }
    }
}
