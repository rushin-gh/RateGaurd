namespace apis.Model
{
    public class UserRateLimitEntry
    {
        public List<DateTime> TimeStamps { get; } = new();
        public object Lock { get; } = new();
    }
}
