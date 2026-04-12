using apis.Contracts;
using apis.Model;
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
    public class RateLimit : ControllerBase
    {
        private readonly IRateLimitService rateLimitService;
        private readonly IDatabase _redisDb;

        public RateLimit(IRateLimitService rateLimitService, IConnectionMultiplexer redis)
        {
            this.rateLimitService = rateLimitService;
            _redisDb = redis.GetDatabase();
        }

        [HttpGet("~/api/test")]
        public async Task<IActionResult> Test([Required] int userId)
        {
            var key = $"RATE:{userId}";

            // Get current count
            var count = await _redisDb.StringGetAsync(key);
            int currentCount = count.HasValue ? (int)count : 0;

            if (currentCount >= 5)
            {
                Response.Headers["Retry-After"] = "30";
                return StatusCode(429);
            }

            await _redisDb.StringIncrementAsync(key);
            return Ok($"API working {userId}");
        }
    }
}
