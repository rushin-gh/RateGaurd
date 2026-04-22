using apis.Contracts;
using apis.Model;
using apis.Scripts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace apis.Middleware
{
    public class RedisRateLimitMiddleware
    {
        private readonly IDatabase _redisDb;
        private readonly RequestDelegate _next;
        private readonly RetryWindowSettings _retryWindowSettings;

        public RedisRateLimitMiddleware(IConnectionMultiplexer redis, RequestDelegate next, IOptions<RetryWindowSettings> retryOptions)
        {
            _next = next;
            _redisDb = redis.GetDatabase();
            _retryWindowSettings = retryOptions.Value;
        }

        public async Task InvokeAsync(HttpContext httpContext, IRateLimitService rateLimitService)
        {
            var userId = httpContext.Request.Headers["UserId"];
            if (!string.IsNullOrEmpty(userId))
            {
                var key = string.Format("RATE:{0}", userId);
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                var result = (RedisResult[])await _redisDb.ScriptEvaluateAsync(
                    RateLimitScripts.RateLimitScript,
                    keys: [key],
                    values: [now, _retryWindowSettings.TimeInSeconds * 1000, _retryWindowSettings.RequestsAllowed]
                );

                var hitCount = (int)result[0];
                var hitLimit = (int)result[1];

                if (hitCount >= hitLimit)
                {
                    var ttl = await _redisDb.KeyTimeToLiveAsync(key);
                    httpContext.Response.Headers["Retry-After"] = ((int)ttl?.TotalSeconds).ToString();
                    httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await httpContext.Response.WriteAsync("Bhag sale");
                }
                else
                {
                    httpContext.Response.OnStarting(() =>
                    {
                        httpContext.Response.Headers["X-RateLimit-Limit"] = hitLimit.ToString();
                        httpContext.Response.Headers["X-RateLimit-Remaining"] = (hitLimit - hitCount).ToString();
                        return Task.CompletedTask;
                    });

                    await _next(httpContext);
                }
            }
            else
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsync("UserId missing in header");
            }
        }
    }
}
