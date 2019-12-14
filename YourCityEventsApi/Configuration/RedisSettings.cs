using System;
using StackExchange.Redis;

namespace YourCityEventsApi.Model
{
    public class RedisSettings:IRedisSettings
    {
        public string Host { get; set; }
        
        public int Port { get; set; }
        
        public string Password { get; set; }

        public static ConnectionMultiplexer GetConnectionMultiplexer()
        {
            Lazy<ConnectionMultiplexer> lazyConnection=new Lazy<ConnectionMultiplexer>(
                ()=> {return ConnectionMultiplexer
                    .Connect("YourCityEvents.redis.cache.windows.net:6380,password=ktK1x6VPHTXr1pb5LwlN+8SaKLWGyCFXTA3mDHL6XPw=,ssl=True,abortConnect=False");});
            return lazyConnection.Value;
        }
    }
    
    public interface IRedisSettings
    {
        string Host { get; set; }
        int Port { get; set; }
        string Password { get; set; }
    }
}