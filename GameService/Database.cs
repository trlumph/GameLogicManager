using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace GameService
{
    public class Database
    {
        private readonly IMongoCollection<ActionLog> _collection;

        public Database(IOptions<DatabaseSettings> databaseSettings)
        {
            var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
            _collection = mongoDatabase.GetCollection<ActionLog>(databaseSettings.Value.CollectionName);
        }

        public async Task<List<ActionLog>> GetAsync() => await _collection.Find(_ => true).ToListAsync();
        public async Task<ActionLog?> GetAsync(string id) => await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(ActionLog newActionLog) => await _collection.InsertOneAsync(newActionLog);
        public async Task UpdateAsync(string id, ActionLog updatedActionLog) => await _collection.ReplaceOneAsync(x => x.Id == id, updatedActionLog);
        public async Task RemoveAsync(string id) => await _collection.DeleteOneAsync(x => x.Id == id);

        public class DatabaseSettings
        {
            public string ConnectionString { get; set; } = null!;
            public string DatabaseName { get; set; } = null!;
            public string CollectionName { get; set; } = null!;
        }

        public class ActionLog
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string? Id { get; set; }

            public string ActionType { get; set; } = null!;
            public string PlayerId { get; set; } = null!;
        }
    }
}