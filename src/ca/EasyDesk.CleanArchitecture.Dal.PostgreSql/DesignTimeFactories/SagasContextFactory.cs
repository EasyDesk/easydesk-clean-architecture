using EasyDesk.CleanArchitecture.Dal.EfCore.Sagas;
using Microsoft.EntityFrameworkCore.Design;

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.DesignTimeFactories;

internal class SagasContextFactory : IDesignTimeDbContextFactory<SagasContext>
{
    public SagasContext CreateDbContext(string[] args)
    {
        var options = DbContextOptionsUtils.CreateDesignTimeOptions<SagasContext>();
        return new(options);
    }
}
