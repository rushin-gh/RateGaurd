namespace apis.Model
{
    public class UserRateLimitEntry
    {
        public Queue<DateTime> TimeStamps { get; } = new();

        public object Lock { get; } = new();
    }
}
