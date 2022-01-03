using MongoDB.Bson.Serialization;

namespace EasyDesk.CleanArchitecture.Dal.Mongo;

public abstract class MongoEntityConfiguration<T> : IMongoConfiguration
{
    public void Apply()
    {
        BsonClassMap.RegisterClassMap<T>(Configure);
    }

    protected abstract void Configure(BsonClassMap<T> classMap);
}
