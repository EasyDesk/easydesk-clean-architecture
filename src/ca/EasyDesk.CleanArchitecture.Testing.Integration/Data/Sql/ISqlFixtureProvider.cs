using DotNet.Testcontainers.Containers;
using System.Data.Common;

namespace EasyDesk.CleanArchitecture.Testing.Integration.Data.Sql;

public interface ISqlFixtureProvider<T> where T : IContainer
{
    string GetConnectionString(T container);

    DbConnection CreateConnection(string connectionString);
}
