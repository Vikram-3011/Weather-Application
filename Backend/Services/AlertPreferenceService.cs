using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using WeatherApi.Models;

namespace WeatherApi.Services
{
    public class AlertPreferenceService
    {
        private readonly IMongoCollection<AlertPreferences> _collection;

        public AlertPreferenceService(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("MongoDB"));
            var db = client.GetDatabase("authentication");
            _collection = db.GetCollection<AlertPreferences>("Alert city");
        }

        public async Task SavePreferencesAsync(string email, List<string> prefs)
        {
            var filter = Builders<AlertPreferences>.Filter.Eq(p => p.UserEmail, email);
            var update = Builders<AlertPreferences>.Update.Set(p => p.Preferences, prefs);
            var options = new UpdateOptions { IsUpsert = true };
            await _collection.UpdateOneAsync(filter, update, options);
        }

        public async Task<List<string>> GetPreferencesAsync(string email)
        {
            var result = await _collection.Find(p => p.UserEmail == email).FirstOrDefaultAsync();
            return result?.Preferences ?? new List<string>();
        }
    }
}

namespace WeatherApi.Models
{
    [BsonIgnoreExtraElements] // ✅ Add this attribute to ignore unknown fields like "Cities"
    public class AlertPreferences
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("UserEmail")]
        public string UserEmail { get; set; } = "";

        [BsonElement("Preferences")]
        public List<string> Preferences { get; set; } = new();
    }
}
