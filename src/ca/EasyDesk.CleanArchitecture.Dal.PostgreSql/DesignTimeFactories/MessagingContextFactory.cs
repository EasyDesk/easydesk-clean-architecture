using EasyDesk.CleanArchitecture.Dal.EfCore.Messaging;
using Microsoft.EntityFrameworkCore.Design;

namespace EasyDesk.CleanArchitecture.Dal.PostgreSql.DesignTimeFactories;

internal class MessagingContextFactory : IDesignTimeDbContextFactory<MessagingContext>
{
    public MessagingContext CreateDbContext(string[] args)
    {
        var options = DbContextOptionsUtils.CreateDesignTimeOptions<MessagingContext>();
        return new MessagingContext(options);
    }
}
