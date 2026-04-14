using apis.Contracts;
using apis.Model;
using apis.Scripts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RateLimitController : ControllerBase
    {
        private readonly RetryWindowSettings retryWindowSettings;
        private readonly IDatabase _redisDb;

        public RateLimitController(IOptions<RetryWindowSettings> retryOptions, IConnectionMultiplexer redis)
        {
            retryWindowSettings = retryOptions.Value;
            _redisDb = redis.GetDatabase();
        }

        [HttpGet("~/api/test")]
        public async Task<IActionResult> Test([Required] int userId)
        {
            var key = $"RATE:{userId}";
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var result = (int)await _redisDb.ScriptEvaluateAsync(
                RateLimitScripts.RateLimitScript,
                keys: [key],
                values: [now, retryWindowSettings.TimeInSeconds * 1000, retryWindowSettings.RequestsAllowed]
            );

            if (result == -1)
            {
                var ttl = await _redisDb.KeyTimeToLiveAsync(key);
                Response.Headers["Retry-After"] = ((int)ttl?.TotalSeconds).ToString();
                return StatusCode(429);
            }

            // Get current count
            //var count = await _redisDb.StringGetAsync(key);
            //int currentCount = count.HasValue ? (int)count : 0;

            //if (currentCount >= 5)
            //{
            //    Response.Headers["Retry-After"] = "30";
            //    return StatusCode(429);
            //}

            return Ok($"API working. Count: {result}/{retryWindowSettings.RequestsAllowed}");
        }
    }
}
