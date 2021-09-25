using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EasyDesk.CleanArchitecture.Dal.Mongo
{
    public class MongoContext : IDisposable
    {
        public delegate Task MongoCommand(IMongoDatabase db, IClientSessionHandle session);

        private readonly IList<MongoCommand> _commands;
        private readonly IMongoClient _client;
        private readonly IMongoDatabase _db;

        public MongoContext(string connectionString, string databaseName)
        {
            _client = new MongoClient(connectionString);
            _db = _client.GetDatabase(databaseName);

            _commands = new List<MongoCommand>();
        }

        public void AddCommand(MongoCommand command) => _commands.Add(command);

        public IMongoCollection<T> GetCollection<T>() => _db.GetCollection<T>(typeof(T).Name);

        public async Task<int> SaveChanges()
        {
            using (var session = await _client.StartSessionAsync())
            {
                session.StartTransaction();

                foreach (var command in _commands)
                {
                    await command(_db, session);
                }

                await session.CommitTransactionAsync();
            }

            var count = _commands.Count;
            _commands.Clear();
            return count;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
