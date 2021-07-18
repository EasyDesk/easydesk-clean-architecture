using MongoDB.Driver;

namespace EasyDesk.CleanArchitecture.Dal.Mongo
{
    public abstract class MongoRepository<T>
        where T : class
    {
        public MongoRepository(MongoContext context)
        {
            Context = context;
            Collection = context.GetCollection<T>();
        }

        protected MongoContext Context { get; }

        protected IMongoCollection<T> Collection { get; }
    }
}
