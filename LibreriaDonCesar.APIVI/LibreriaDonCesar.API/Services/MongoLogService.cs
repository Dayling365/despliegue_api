using LibreriaDonCesar.API.Configurations;
using LibreriaDonCesar.API.Models;
using LibreriaDonCesar.Core.Common;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace LibreriaDonCesar.API.Services;

public interface ILogService
{
    Task InsertLogAsync(SystemLog log);
    Task<PagedResponse<List<SystemLog>>> GetLogsAsync(
        int pageNumber,
        int pageSize
    );
}

public class MongoLogService : ILogService
{
    private readonly IMongoCollection<SystemLog> _collection;

    public MongoLogService(IOptions<MongoLogSettings> options)
    {
        var settings = options.Value;
        var clientSettings =
            MongoClientSettings.FromConnectionString(settings.ConnectionString);
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
            Console.WriteLine(
                $"[WARNING] No se pudieron crear los índices de MongoDB: {ex.Message}"
            );
        }
    }

    private void InicializarIndices(int retentionDays)
    {
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
            Console.WriteLine(
                $"[CRITICAL] Fallo de persistencia en MongoDB: {ex.Message}"
            );
        }
    }

    public async Task<PagedResponse<List<SystemLog>>> GetLogsAsync(
        int pageNumber,
        int pageSize
    )
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var filter = Builders<SystemLog>.Filter.Empty;
        var totalRecords = await _collection.CountDocumentsAsync(filter);

        var logs = await _collection
            .Find(filter)
            .SortByDescending(log => log.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        return new PagedResponse<List<SystemLog>>
        {
            Data = logs,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = (int)totalRecords
        };
    }
}
