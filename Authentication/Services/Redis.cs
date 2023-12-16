using StackExchange.Redis;
using Newtonsoft.Json;
using Workout.Authentication.Interfaces;
using Workout.Authentication.Models;

namespace Workout.Authentication.Services;

public class RedisCacheService : ICacheService
{
    private readonly ConnectionMultiplexer _redisConnection;
    private readonly IDatabase _database;
    private readonly TimeSpan _sessionExpiry;
    public RedisCacheService(ConfigProvider configProvider)
    {
        _redisConnection = ConnectionMultiplexer.Connect(configProvider.RedisConnectionString);
        _database = _redisConnection.GetDatabase();
        _sessionExpiry = configProvider.SessionExpiry;
    }

    public async Task<string> SaveUserAsync(User user, HttpContext context)
    {
        // create session
        Session session = new()
        {
            UserId = user.Id!,
            UserName = user.UserName,
            Role = user.Role,
            IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? string.Empty,
            UserAgent = context.Request.Headers["User-Agent"].ToString(),
            NoteRecordId = user.NoteRecordId,
            LastSeen = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            ExpireAt = DateTime.UtcNow.Add(_sessionExpiry),
        };
        string sessionId = user.Id!;

        // serialize session
        string sessionJson = JsonConvert.SerializeObject(session);

        // save session to redis
        await _database.StringSetAsync(sessionId, sessionJson, _sessionExpiry);

        return sessionId;
    }

    public async Task<Session?> GetSessionByIdAsync(string sessionId)
    {
        string? sessionJson = await _database.StringGetAsync(sessionId);

        if (sessionJson == null)
        {
            return null;
        }

        return JsonConvert.DeserializeObject<Session>(sessionJson);
    }

    public async Task UpdateSessionByIdAsync(string sessionId, Session session)
    {
        string sessionJson = JsonConvert.SerializeObject(session);
        await _database.StringSetAsync(sessionId, sessionJson, _sessionExpiry);
    }

    public async Task DeleteSessionByIdAsync(string sessionId)
    {
        await _database.KeyDeleteAsync(sessionId);
    }
}