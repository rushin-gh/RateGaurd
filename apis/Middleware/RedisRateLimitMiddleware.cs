using apis.Contracts;
using StackExchange.Redis;

namespace apis.Middleware
{
    public class RedisRateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IDatabase _redisDb;

        public RedisRateLimitMiddleware(IConnectionMultiplexer redis, RequestDelegate next)
        {
            _redisDb = redis.GetDatabase();
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, IRateLimitService rateLimitService)
        {
            bool isUserIdPassedInHeader = httpContext.Request.Headers.Any(x => string.Compare(x.Key, "UserId") == 0);
            if (isUserIdPassedInHeader)
            {
                await _next(httpContext);
            }
            else
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsync("UserId missing in header");
            }            
        }
    }
}
