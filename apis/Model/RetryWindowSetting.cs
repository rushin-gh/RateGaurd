namespace apis.Model
{
    public class RetryWindowSettings
    {
        public int TimeInSeconds { get; set; }
        public int RequestsAllowed { get; set; }  // also fixed the typo from appsettings
    }
}
