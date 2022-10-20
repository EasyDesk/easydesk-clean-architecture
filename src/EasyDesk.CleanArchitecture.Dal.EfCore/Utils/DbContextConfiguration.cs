using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public class DbContextConfiguration
{
    public DbContextConfiguration(
        Type dbContextType,
        SqlConnection connection,
        DbContextOptionsBuilder options,
        string schema)
    {
        DbContextType = dbContextType;
        Connection = connection;
        Options = options;
        Schema = schema;
    }

    public Type DbContextType { get; }

    public SqlConnection Connection { get; }

    public DbContextOptionsBuilder Options { get; }

    public string Schema { get; }
}
