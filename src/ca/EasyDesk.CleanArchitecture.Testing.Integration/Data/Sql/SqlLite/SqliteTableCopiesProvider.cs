using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.Sqlite;

internal class SqliteTableCopiesProvider : ITableCopiesProvider
{
    public string GenerateTablesQuery(string schemaOutput, string tableOutput, string columnOutput) => $"""
        SELECT '' AS "{schemaOutput}", M.name AS "{tableOutput}", P.name AS "{columnOutput}"
        FROM sqlite_master M
        LEFT OUTER JOIN pragma_table_info((M.name)) P
             on M.name <> P.name
        WHERE M.type = 'table'
        ORDER BY M.name, P.name;
        """;

    public string GenerateCopyTableCommand(TableDef table) => $"""
        CREATE TABLE {FormatCopyTable(table)}
        AS SELECT * FROM {FormatTable(table)};
        """;

    public string GenerateDisableConstraintsCommand(TableDef table) => """
        PRAGMA ignore_check_constraints = true;
        """;

    public string GenerateEnableConstraintsCommand(TableDef table) => """
        PRAGMA ignore_check_constraints = false;
        """;

    public string GenerateRestoreTableCommand(TableDef table)
    {
        var columnsList = table.Columns.Select(x => $@"""{x}""").ConcatStrings(", ");
        return $"""
            DELETE FROM {FormatTable(table)};

            INSERT INTO {FormatTable(table)} ({columnsList})
            SELECT {columnsList}
            FROM {FormatCopyTable(table)};
            """;
    }

    private string FormatCopyTable(TableDef table) => FormatTableRaw($"__copy_{table.Name}__");

    private string FormatTable(TableDef table) => FormatTableRaw(table.Name);

    private string FormatTableRaw(string name) => $@"""{name}""";
}
