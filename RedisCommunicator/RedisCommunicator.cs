using System.Text.Json;
using StackExchange.Redis;

namespace RedisCommunicator;

public class RedisCommunicator : IRedisCommunicator
{
    private readonly IConnectionMultiplexer _redis;

    public RedisCommunicator(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task SetCacheValueAsync<T>(string key, T value, TimeSpan expiration)
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(value);
        await db.StringSetAsync(key, json, expiration);
    }

    public async Task<T> GetCacheValueAsync<T>(string key)
    {
        var db = _redis.GetDatabase();
        var json = await db.StringGetAsync(key);
        return json.HasValue ? JsonSerializer.Deserialize<T>(json) : default;
    }

    public bool DeleteValue(string key)
    {
        var db = _redis.GetDatabase();
        return db.KeyDelete(key);
    }
}

public interface IRedisCommunicator
{
    Task SetCacheValueAsync<T>(string key, T value, TimeSpan expiration);
    Task<T> GetCacheValueAsync<T>(string key);
    bool DeleteValue(string key);
}