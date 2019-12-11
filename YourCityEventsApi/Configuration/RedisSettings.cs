using System;
using ImageMagick;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http2.HPack;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using StackExchange.Redis;

namespace YourCityEventsApi.Model
{
    public class RedisSettings:IRedisSettings
    {
        public string Host { get; set; }
        
        public int Port { get; set; }
        
        public string Password { get; set; }

        public static IDatabase ConnectToDatabase(IRedisSettings redisSettings,int databaseIndex)
        {
            ConfigurationOptions options = new ConfigurationOptions()
            {    
                EndPoints = {{redisSettings.Host,redisSettings.Port}},
                Password = redisSettings.Password
            };
            ConnectionMultiplexer redis=ConnectionMultiplexer.Connect(options);
            
            return redis.GetDatabase(databaseIndex);
        }

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