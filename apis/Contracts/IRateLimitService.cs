using apis.Model;

namespace apis.Contracts
{
    public interface IRateLimitService
    {
        RateLimitResult GetRateLimitResult(int userId);
    }
}