using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace EasyDesk.CleanArchitecture.Dal.EfCore.Utils;

public static class MigrationUtils
{
    public static OperationBuilder<SqlOperation> UnsafeSql(this MigrationBuilder migrationBuilder, string sql)
    {
        return migrationBuilder.Sql($"EXEC sp_executesql N'{sql.Replace("'", "''")}'");
    }
}
