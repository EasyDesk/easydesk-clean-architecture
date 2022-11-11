using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public class AbstractDbContext<T> : DbContext
    where T : AbstractDbContext<T>
{
    private readonly IList<DbContextExtension> _extensions = new List<DbContextExtension>();

    public AbstractDbContext(DbContextOptions<T> options) : base(options)
    {
    }

    internal void AddExtension(DbContextExtension extension)
    {
        extension.Initialize(this);
        _extensions.Add(extension);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _extensions
            .Reverse()
            .Aggregate(base.OnModelCreating, (curr, ext) => mb => ext.CreateModel(mb, () => curr(mb)))(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _extensions
            .Reverse()
            .Aggregate<DbContextExtension, AsyncFunc<CancellationToken, int>>(
                base.SaveChangesAsync,
                (curr, ext) => ct => ext.SaveChanges(() => curr(ct)))(cancellationToken);
    }
}
