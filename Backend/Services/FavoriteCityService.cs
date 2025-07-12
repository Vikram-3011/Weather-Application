using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using WeatherApi.Models;
namespace WeatherApi.Services
{
    public class FavoriteCityService
    {
        private readonly IMongoCollection<FavoriteCitiesModel> _collection;

        public FavoriteCityService(IOptions<MongoDbSettings> options)
        {
            var client = new MongoClient(options.Value.ConnectionString);
            var database = client.GetDatabase("authentication");
            _collection = database.GetCollection<FavoriteCitiesModel>("fav cities");
        }

        // Store (Add) city
        public async Task AddCityAsync(string email, string city)
        {
            var filter = Builders<FavoriteCitiesModel>.Filter.Eq(f => f.UserEmail, email);
            var update = Builders<FavoriteCitiesModel>.Update.AddToSet(f => f.Cities, city);
            var options = new UpdateOptions { IsUpsert = true };

            await _collection.UpdateOneAsync(filter, update, options);
        }

        // Retrieve all cities
        public async Task<List<string>> GetCitiesAsync(string email)
        {
            var filter = Builders<FavoriteCitiesModel>.Filter.Regex(
                "UserEmail",
                new MongoDB.Bson.BsonRegularExpression($"^{email}$", "i")
            );

            var userCities = await _collection.Find(filter).FirstOrDefaultAsync();

            return userCities?.Cities ?? new List<string>();
        }

        // Delete specific city
        public async Task RemoveCityAsync(string email, string city)
        {
            var filter = Builders<FavoriteCitiesModel>.Filter.Eq(f => f.UserEmail, email);
            var update = Builders<FavoriteCitiesModel>.Update.Pull(f => f.Cities, city);
            await _collection.UpdateOneAsync(filter, update);
        }
    }
}



[BsonIgnoreExtraElements] // Ignore unrelated fields like "Preferences"
public class FavoriteCitiesModel
{
    public string UserEmail { get; set; }
    public List<string> Cities { get; set; }
}



namespace WeatherApi.Models
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
    }
}
