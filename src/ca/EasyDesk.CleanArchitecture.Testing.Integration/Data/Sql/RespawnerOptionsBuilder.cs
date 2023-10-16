using Respawn;
using Respawn.Graph;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;

public class RespawnerOptionsBuilder
{
    private readonly IDbAdapter _dbAdapter;

    private readonly ISet<Table> _tablesToExclude = new HashSet<Table>();
    private readonly ISet<Table> _tablesToInclude = new HashSet<Table>();
    private readonly ISet<string> _schemasToExclude = new HashSet<string>();
    private readonly ISet<string> _schemasToInclude = new HashSet<string>();
    private int? _commandTimeout = null;
    private bool _checkTemporalTables = false;
    private bool _withReseed = false;

    public RespawnerOptionsBuilder(IDbAdapter dbAdapter)
    {
        _dbAdapter = dbAdapter;
    }

    public RespawnerOptionsBuilder ExcludeSchemas(params string[] schemas)
    {
        _schemasToExclude.UnionWith(schemas);
        return this;
    }

    public RespawnerOptionsBuilder IncludeSchemas(params string[] schemas)
    {
        _schemasToInclude.UnionWith(schemas);
        return this;
    }

    public RespawnerOptionsBuilder ExcludeTables(params Table[] tables)
    {
        _tablesToExclude.UnionWith(tables);
        return this;
    }

    public RespawnerOptionsBuilder IncludeTables(params Table[] tables)
    {
        _tablesToInclude.UnionWith(tables);
        return this;
    }

    public RespawnerOptionsBuilder WithCommandTimeout(int timeout)
    {
        _commandTimeout = timeout;
        return this;
    }

    public RespawnerOptionsBuilder WithoutCommandTimeout()
    {
        _commandTimeout = null;
        return this;
    }

    public RespawnerOptionsBuilder WithReseed(bool reseed = true)
    {
        _withReseed = reseed;
        return this;
    }

    public RespawnerOptionsBuilder CheckTemporalTables(bool check = true)
    {
        _checkTemporalTables = check;
        return this;
    }

    internal RespawnerOptions Build() => new()
    {
        TablesToIgnore = _tablesToExclude.ToArray(),
        TablesToInclude = _tablesToInclude.ToArray(),
        SchemasToExclude = _schemasToExclude.ToArray(),
        SchemasToInclude = _schemasToInclude.ToArray(),
        DbAdapter = _dbAdapter,
        WithReseed = _withReseed,
        CheckTemporalTables = _checkTemporalTables,
        CommandTimeout = _commandTimeout,
    };
}
