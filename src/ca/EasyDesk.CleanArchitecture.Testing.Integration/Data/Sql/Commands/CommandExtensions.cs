using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql.Commands;

public static class CommandExtensions
{
    public static async Task RunCommand(this DbConnection connection, string text, Action<DbCommand>? configureCommand = null)
    {
        await using var command = connection.CreateCommand(text, configureCommand);
        await command.ExecuteNonQueryAsync();
    }

    public static async IAsyncEnumerable<T> RunQuery<T>(this DbConnection connection, string text, Func<DbDataReader, T> parser, Action<DbCommand>? configureCommand = null)
    {
        await using var command = connection.CreateCommand(text, configureCommand);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            yield return parser(reader);
        }
    }

    private static DbCommand CreateCommand(this DbConnection connection, string text, Action<DbCommand>? configureCommand)
    {
        var command = connection.CreateCommand();
        command.CommandText = text;
        configureCommand?.Invoke(command);
        return command;
    }
}
