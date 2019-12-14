namespace YourCityEventsApi
{
    public class JwtSettings : IJwtSettings
    {
        public string Secret { get; set; }
    }

    public interface IJwtSettings
    {
        string Secret { get; set; }
    }
}