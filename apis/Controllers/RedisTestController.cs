using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace apis.Controllers
{
    public class RedisTestController : ControllerBase
    {
        private readonly IDatabase _db;

        public RedisTestController(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        [HttpGet("~/api/redis-test")]
        public async Task<IActionResult> Test()
        {
            // Store
            await _db.StringSetAsync("test-key", "hello redis");

            // Retrieve
            var value = await _db.StringGetAsync("test-key");

            return Ok(value.ToString());  // "hello redis"
        }
    }
}
