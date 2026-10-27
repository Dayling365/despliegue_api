using LibreriaDonCesar.API.Configurations;
using LibreriaDonCesar.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
namespace LibreriaDonCesar.API.Services;
public interface ILogService
{
    Task InsertLogAsync(SystemLog log);
}
public class MongoLogService : ILogService
{
    private readonly IMongoCollection<SystemLog> _collection;
    public MongoLogService(IOptions<MongoLogSettings> options)
    {
        var settings = options.Value;
        var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
        clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
        var client = new MongoClient(clientSettings);
        var database = client.GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<SystemLog>(settings.CollectionName);
        try
        {
            InicializarIndices(settings.RetentionDays);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] No se pudieron crear los índices de MongoDB: {ex.Message}");
        }
    }
    private void InicializarIndices(int retentionDays)
    {
        // Cosmos DB for MongoDB (RU) solo admite índices TTL sobre "_ts"
        var ttlKeys = Builders<SystemLog>.IndexKeys.Ascending("_ts");
        var ttlOptions = new CreateIndexOptions 
        { 
            ExpireAfter = TimeSpan.FromDays(retentionDays) 
        };
        var traceKeys = Builders<SystemLog>.IndexKeys.Ascending(x => x.TraceId);
        var levelKeys = Builders<SystemLog>.IndexKeys.Ascending(x => x.Level);
        _collection.Indexes.CreateMany([
            new CreateIndexModel<SystemLog>(ttlKeys, ttlOptions),
            new CreateIndexModel<SystemLog>(traceKeys),
            new CreateIndexModel<SystemLog>(levelKeys)
        ]);
    }
    public async Task InsertLogAsync(SystemLog log)
    {
        try
        {
            await _collection.InsertOneAsync(log);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CRITICAL] Fallo de persistencia en MongoDB: {ex.Message}");
        }
    }
}
