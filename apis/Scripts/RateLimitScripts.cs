namespace apis.Scripts
{
    public static class RateLimitScripts
    {
        public static string RateLimitScript = @"
            local key     = KEYS[1]
            local now     = tonumber(ARGV[1])
            local window  = tonumber(ARGV[2])
            local limit   = tonumber(ARGV[3])
            local cutoff  = now - window

            -- Remove timestamps outside the window
            redis.call('ZREMRANGEBYSCORE', key, '-inf', cutoff)

            -- Count requests inside window
            local count = redis.call('ZCARD', key)

            if count >= limit then
                return {count, limit}  -- blocked, but still return limit
            end

            -- Add current request with timestamp as score
            redis.call('ZADD', key, now, now)

            -- Set expiry so key auto-cleans
            redis.call('EXPIRE', key, window)

            return {count + 1, limit}  -- ✅ returns tuple of (current count, limit)
        ";
    }
}
