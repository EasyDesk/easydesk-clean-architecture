using EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.Commands;
using EasyDesk.Commons.Collections;
using EasyDesk.Commons.Tasks;
using System.Data;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;

public class TableCopiesManager
{
    private const string SchemaOutput = "SCHEMA";
    private const string TableOutput = "TABLE";
    private const string ColumnOutput = "COLUMN";

    private readonly Func<DbConnection> _connectionFactory;
    private readonly ITableCopiesProvider _provider;

    private string _restoreTablesCommand = default!;

    public TableCopiesManager(Func<DbConnection> connectionFactory, ITableCopiesProvider provider)
    {
        _connectionFactory = connectionFactory;
        _provider = provider;
    }

    public async Task PrepareTableCopies()
    {
        await UsingDbConnection(async connection =>
        {
            var tablesQuery = _provider.GenerateTablesQuery(SchemaOutput, TableOutput, ColumnOutput);

            var result = await connection
                .RunQuery(tablesQuery, x => new
                {
                    Schema = x.GetString(SchemaOutput),
                    TableName = x.GetString(TableOutput),
                    ColumnName = x.GetString(ColumnOutput),
                })
                .ToEnumerableAsync();

            var tables = result.GroupBy(
                x => (x.Schema, x.TableName),
                (k, xs) => new TableDef(k.Schema, k.TableName, xs.Select(c => c.ColumnName).ToFixedList()));

            var copyTablesCommand = tables.Select(_provider.GenerateCopyTableCommand).ConcatStrings("\n");
            await connection.RunCommand(copyTablesCommand);

            _restoreTablesCommand = tables
                .Select(_provider.GenerateDisableConstraintsCommand)
                .Concat(tables.Select(_provider.GenerateRestoreTableCommand))
                .Concat(tables.Select(_provider.GenerateEnableConstraintsCommand))
                .ConcatStrings("\n");
        });
    }

    public async Task RestoreDataFromTableCopies()
    {
        await UsingDbConnection(async connection =>
        {
            await connection.RunCommand(_restoreTablesCommand);
        });
    }

    private async Task UsingDbConnection(AsyncAction<DbConnection> action)
    {
        await using var connection = _connectionFactory();
        await connection.OpenAsync();
        await action(connection);
    }
}
