namespace apis.Model
{
    public class RateLimitResult
    {
        public bool IsAllowed { get; set; }

        public int TimesInSeconds { get; set; }

        public RateLimitResult()
        {
            IsAllowed = default;
            TimesInSeconds = default;
        }

        public RateLimitResult(bool isAllowed, int timesInSeconds)
        {
            IsAllowed = isAllowed;
            TimesInSeconds = timesInSeconds;
        }
    }
}
