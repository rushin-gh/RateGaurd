using apis.Contracts;

namespace apis.Middleware
{
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;

        public RateLimitMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, IRateLimitService rateLimitService)
        {
            if (!TryGetUserId(httpContext, out int userId))
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsync("Missing or invalid user identifier.");
                return;
            }

            var result = rateLimitService.GetRateLimitResult(userId);

            if (!result.IsAllowed)
            {
                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                httpContext.Response.Headers["Retry-After"] = Convert.ToString(result.TimesInSeconds);
                await httpContext.Response.WriteAsync($"Too many requests for {userId}.");
                return;
            }

            await _next(httpContext);
        }

        private static bool TryGetUserId(HttpContext context, out int userId)
        {
            // Example: reading from a header — swap for claims/JWT as needed
            var raw = context.Request.Query.FirstOrDefault(item => string.Compare(item.Key, "userid", true) == 0).Value;
            return int.TryParse(raw, out userId);
        }
    }
}
