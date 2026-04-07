using apis.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace apis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RateLimit : ControllerBase
    {
        private readonly ILogger<RateLimit> _logger;

        static readonly ConcurrentDictionary<int, UserRateLimitEntry> userRequests = new();

        private readonly int _timeInSeconds;
        private readonly int _requestsAllowed;

        public RateLimit(IOptions<RetryWindowSettings> retryOptions, ILogger<RateLimit> logger)
        {
            _timeInSeconds = retryOptions.Value.TimeInSeconds;
            _requestsAllowed = retryOptions.Value.RequestsAllowed;
            _logger = logger;
        }

        [HttpGet("~/api/test")]
        public IActionResult Test([Required] int userId)
        {
            var entry = userRequests.GetOrAdd(userId, _ => new UserRateLimitEntry());
            lock (entry.Lock)
            {
                var cutoff = DateTime.UtcNow.AddSeconds(-_timeInSeconds);

                // Clean up old timestamps (fixes memory leak too)
                while (entry.TimeStamps.Count > 0 && entry.TimeStamps.Peek() < cutoff)
                {
                    entry.TimeStamps.Dequeue();
                }

                if (entry.TimeStamps.Count >= _requestsAllowed)
                {
                    Response.Headers["Retry-After"] = _timeInSeconds.ToString();
                    return StatusCode(429);
                }

                entry.TimeStamps.Enqueue(DateTime.UtcNow);
            }

            return Ok($"API working {userId}");
        }
    }
}
