using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json;
using StackExchange.Redis;
using YourCityEventsApi.Model;

namespace YourCityEventsApi.ScheduleTask
{
    public interface IScopedService
    {
        Task SyncData(CancellationToken cancellationToken);
    }

    public class ScopedService:IScopedService
    {
        private readonly IMongoCollection<BackendUserModel> _users;
        private readonly IMongoCollection<BackendEventModel> _events;
        private readonly IMongoCollection<CityModel> _cities;
        private IDatabase _redisUsersDatabase;
        private IDatabase _redisEventsDatabase;
        private IDatabase _redisCitiesDatabase;
        
        public ScopedService(IMongoSettings mongoSettings)
        {
            var client=new MongoClient(mongoSettings.ConnectionString);
            var database = client.GetDatabase(mongoSettings.DatabaseName);
            _users = database.GetCollection<BackendUserModel>("Users");
            _events = database.GetCollection<BackendEventModel>("Events");
            _cities = database.GetCollection<CityModel>("Cities");
            
            var redis = RedisSettings.GetConnectionMultiplexer();
            _redisUsersDatabase = redis.GetDatabase(0);
            _redisEventsDatabase = redis.GetDatabase(1);
            _redisCitiesDatabase = redis.GetDatabase(2);
        }
        
        public async Task SyncData(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TimeSpan ttl = new TimeSpan(0, 1, 59, 59);

                var allUsers = _users.Find(u => true).ToList();
                var allEvents = _events.Find(e => true).ToList();
                var allCities = _cities.Find(c => true).ToList();

                foreach (var user in allUsers)
                {
                    _redisUsersDatabase.StringSet(user.Id, JsonConvert.SerializeObject(user), ttl);
                }

                foreach (var Event in allEvents)
                {
                    _redisEventsDatabase.StringSet(Event.Id, JsonConvert.SerializeObject(Event), ttl);
                }

                foreach (var city in allCities)
                {
                    _redisCitiesDatabase.StringSet(city.Id, JsonConvert.SerializeObject(city), ttl);
                }

                await Task.Delay(2*60*60* 1000, cancellationToken);
            }
        }
    }
}