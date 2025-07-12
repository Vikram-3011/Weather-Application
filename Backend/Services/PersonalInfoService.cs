using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WeatherApi.Models;

public class PersonalInfoService
{
    private readonly IMongoCollection<PersonalInfo> _collection;

    public PersonalInfoService(IOptions<MongoDbSettings> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        var database = client.GetDatabase("authentication");
        _collection = database.GetCollection<PersonalInfo>("User details");
    }

    public async Task<PersonalInfo?> GetByEmailAsync(string email) =>
        await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();

    public async Task<bool> UpsertAsync(PersonalInfo info)
    {
        var filter = Builders<PersonalInfo>.Filter.Eq(u => u.Email, info.Email);
        var result = await _collection.ReplaceOneAsync(filter, info, new ReplaceOptions { IsUpsert = true });
        return result.IsAcknowledged;
    }
}
public class PersonalInfo
{
    public string Id { get; set; } = string.Empty; // Optional
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

