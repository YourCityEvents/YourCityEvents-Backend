using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using Newtonsoft.Json;
using StackExchange.Redis;
using YourCityEventsApi.Model;


namespace YourCityEventsApi.ScheduleTask
{
    public class SyncDataService:BackgroundService
    {
        private readonly IMongoCollection<UserModel> _users;
        private readonly IMongoCollection<EventModel> _events;
        private readonly IMongoCollection<CityModel> _cities;
        private IDatabase _redisUsersDatabase;
        private IDatabase _redisEventsDatabase;
        private IDatabase _redisCitiesDatabase;
        private Timer _timer;

        public SyncDataService(IMongoSettings mongoSettings)
        {
            var client=new MongoClient(mongoSettings.ConnectionString);
            var database = client.GetDatabase(mongoSettings.DatabaseName);
            _users = database.GetCollection<UserModel>("Users");
            _events = database.GetCollection<EventModel>("Events");
            _cities = database.GetCollection<CityModel>("Cities");
            
            var redis = RedisSettings.GetConnectionMultiplexer();
            _redisUsersDatabase = redis.GetDatabase(0);
            _redisEventsDatabase = redis.GetDatabase(1);
            _redisCitiesDatabase = redis.GetDatabase(2);
        }
        
        private void ScheduleTask()
        {
            TimeSpan ttl = new TimeSpan(0,1,59,0);

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
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ScheduleTask();

                await Task.Delay(new TimeSpan(0, 0, 0, 1), cancellationToken);
            }
        }
    }
}