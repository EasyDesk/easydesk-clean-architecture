using EasyDesk.Commons.Collections;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.Postgres;

internal class PostgresTableCopiesProvider : ITableCopiesProvider
{
    public string GenerateTablesQuery(string schemaOutput, string tableOutput, string columnOutput) => $"""
        SELECT T.table_schema AS "{schemaOutput}", T.table_name AS "{tableOutput}", C.column_name AS "{columnOutput}"
        FROM information_schema.tables T
        JOIN information_schema.columns C
        ON T.table_schema = C.table_schema AND T.table_name = C.table_name
        WHERE T.table_type = 'BASE TABLE'
        AND T.table_schema NOT IN ('pg_catalog', 'information_schema');
        """;

    public string GenerateCopyTableCommand(TableDef table) => $"""
        CREATE TABLE {FormatCopyTable(table)}
        AS TABLE {FormatTable(table)};
        """;

    public string GenerateDisableConstraintsCommand(TableDef table) => $"""
        ALTER TABLE {FormatTable(table)} DISABLE TRIGGER ALL;
        """;

    public string GenerateEnableConstraintsCommand(TableDef table) => $"""
        ALTER TABLE{FormatTable(table)} ENABLE TRIGGER ALL;
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

    private string FormatCopyTable(TableDef table) => FormatTableRaw(table.Schema, $"__copy_{table.Name}__");

    private string FormatTable(TableDef table) => FormatTableRaw(table.Schema, table.Name);

    private string FormatTableRaw(string schema, string name) => $@"""{schema}"".""{name}""";
}
