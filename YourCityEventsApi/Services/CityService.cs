using System;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StackExchange.Redis;
using YourCityEventsApi.Model;

namespace YourCityEventsApi.Services
{
    public class CityService
    {
        private IMongoCollection<UserModel> _users;
        private IMongoCollection<EventModel> _events;
        private IMongoCollection<CityModel> _cities;
        private IDatabase _redisCitiesDatabase;
        private IDatabase _redisUsersDatabase;
        private IDatabase _redisEventsDatabase;
        private readonly IEnumerable<RedisKey> _keys;
        private readonly TimeSpan ttl = new TimeSpan(0,1 , 59, 59);

        public CityService(IMongoSettings settings)
        {
            var client=new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _cities = database.GetCollection<CityModel>("Cities");
            _users = database.GetCollection<UserModel>("Users");
            _events = database.GetCollection<EventModel>("Events");

            var redis = RedisSettings.GetConnectionMultiplexer();
            _redisUsersDatabase = redis.GetDatabase(0);
            _redisEventsDatabase = redis.GetDatabase(1);
            _redisCitiesDatabase = redis.GetDatabase(2);
            _keys = redis.GetServer(_redisCitiesDatabase.Multiplexer.GetEndPoints().First()).Keys(2);
        }

        public List<CityModel> GetAll()
        {
            var allCities = new List<CityModel>();
            foreach (var key in _keys)
            {
                allCities.Add(JsonConvert.DeserializeObject<CityModel>(_redisCitiesDatabase.StringGet(key)));
            }

            return allCities;
        }

        public CityModel Get(string id)
        {
            foreach (var key in _keys)
            {
                var city = JsonConvert.DeserializeObject<CityModel>(_redisCitiesDatabase.StringGet(key));
                if (city.Id == id)
                {
                    return city;
                }
            }

            return null;
        }

        public CityModel GetByNameUa(string cityNameUa)
        {
            foreach (var key in _keys)
            {
                var city = JsonConvert.DeserializeObject<CityModel>(_redisCitiesDatabase.StringGet(key));
                if (city.NameUa == cityNameUa)
                {
                    return city;
                }
            }

            return null;
        }
        
        public CityModel GetByNameEn(string cityNameEn)
        {
            foreach (var key in _keys)
            {
                var city = JsonConvert.DeserializeObject<CityModel>(_redisCitiesDatabase.StringGet(key));
                if (city.NameEn == cityNameEn)
                {
                    return city;
                }
            }

            return null;
        }

        public CityModel Create(CityModel cityModel)
        {
            _cities.InsertOne(cityModel);
            var city = GetByNameUa(cityModel.NameUa);
            _redisCitiesDatabase.StringSet(city.Id, JsonConvert.SerializeObject(city), ttl);
            return city;
        }

        public void Update(string id, CityModel cityModel)
        {
            var users = _users.Find(u => true).ToList();
            var events = _events.Find(e => true).ToList();

            foreach (var user in users)
            {
                if (user.City.Id == id)
                {
                    user.City = cityModel;;
                    _users.ReplaceOne(user.Id, user);
                    _redisUsersDatabase.StringSet(user.Id, JsonConvert.SerializeObject(user), ttl);
                }
            }

            foreach (var Event in events)
            {
                if (Event.Location.Id == id)
                {
                    Event.Location = cityModel;
                    _events.ReplaceOne(Event.Id, Event);
                    _redisEventsDatabase.StringSet(Event.Id, JsonConvert.SerializeObject(Event), ttl);
                }
            }
            _cities.ReplaceOne(city => city.Id == id, cityModel);
            _redisCitiesDatabase.StringSet(id, JsonConvert.SerializeObject(cityModel), ttl);
        }

        public void Delete(string id)
        {
            _cities.DeleteOne(city => city.Id == id);
            _redisCitiesDatabase.KeyDelete(id);
        }
    }
}