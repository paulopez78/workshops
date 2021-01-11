using System;
using System.Data;
using System.Threading.Tasks;
using Meetup.Scheduling.Domain;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Meetup.Scheduling.Infrastructure
{
    public class MongoDbRepository<T> where T : Aggregate
    {
        readonly IMongoCollection<T> DbCollection;

        public MongoDbRepository(IMongoDatabase database) =>
            DbCollection = database.GetCollection<T>(typeof(T).Name);

        public Task<T?> Load(string id)
            => DbCollection.AsQueryable().Where(x => x.Id == id).SingleAsync()!;

        public async Task<string> Save(T aggregate)
        {
            var originalVersion = aggregate.Version;
            aggregate.IncreaseVersion();

            var result = await DbCollection.ReplaceOneAsync(x => x.Id == aggregate.Id && x.Version == originalVersion,
                aggregate,
                new ReplaceOptions {IsUpsert = true});

            // https: //jimmybogard.com/document-level-optimistic-concurrency-in-mongodb/
            if (originalVersion > 0 && result.ModifiedCount == 0)
                throw new DBConcurrencyException("Trying to replace a document with wrong version");

            return aggregate.Id;
        }
    }
}