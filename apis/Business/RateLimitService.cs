using apis.Contracts;
using apis.Controllers;
using apis.Model;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace apis.Business
{
    public class RateLimitService : IRateLimitService
    {
        static readonly ConcurrentDictionary<int, UserRateLimitEntry> userRequests = new();

        private readonly int _timeInSeconds;
        private readonly int _requestsAllowed;

        public RateLimitService(IOptions<RetryWindowSettings> retryOptions, ILogger<RateLimitController> logger)
        {
            _timeInSeconds = retryOptions.Value.TimeInSeconds;
            _requestsAllowed = retryOptions.Value.RequestsAllowed;
        }

        public RateLimitResult GetRateLimitResult(int userId)
        {
            var Result = new RateLimitResult(true, _timeInSeconds);
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
                        Result.IsAllowed = false;
                    } 
                    else
                    {
                        entry.TimeStamps.Enqueue(DateTime.UtcNow);
                    }
                }
            }
            return Result;            
        }
    }
}
