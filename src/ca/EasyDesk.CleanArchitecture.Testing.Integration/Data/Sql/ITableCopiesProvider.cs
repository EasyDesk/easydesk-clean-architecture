namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;

public interface ITableCopiesProvider
{
    string GenerateTablesQuery(string schemaOutput, string tableOutput, string columnOutput);

    string GenerateCopyTableCommand(TableDef table);

    string GenerateDisableConstraintsCommand(TableDef table);

    string GenerateEnableConstraintsCommand(TableDef table);

    string GenerateRestoreTableCommand(TableDef table);
}
