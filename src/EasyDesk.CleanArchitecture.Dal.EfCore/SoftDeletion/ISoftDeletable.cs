namespace EasyDesk.CleanArchitecture.Dal.EfCore.SoftDeletion;

public interface ISoftDeletable
{
    public bool IsDeleted { get; set; }
}
