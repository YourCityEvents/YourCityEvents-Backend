using MongoDB.Driver;
using System.Collections.Generic;
using YourCityEventsApi.Model;

namespace YourCityEventsApi.Services
{
    public class CityService
    {
        private IMongoCollection<CityModel> _cities;
        private IMongoCollection<UserModel> _users;
        private IMongoCollection<EventModel> _events;

        public CityService(IDatabaseSettings settings)
        {
            var client=new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _cities = database.GetCollection<CityModel>("Cities");
            _users = database.GetCollection<UserModel>("Users");
            _events = database.GetCollection<EventModel>("Events");
        }

        public List<CityModel> Get()
        {
            return _cities.Find(city => true).ToList();
        }

        public CityModel Get(string id)
        {
            return _cities.Find(city => city.Id == id).FirstOrDefault();
        }

        public CityModel GetByNameUa(string cityNameUa)
        {
            return _cities.Find(city => city.NameUa == cityNameUa)
                .FirstOrDefault();
        }
        
        public CityModel GetByNameEn(string cityNameEn)
        {
            return _cities.Find(city => city.NameEn == cityNameEn)
                .FirstOrDefault();
        }

        public CityModel Create(CityModel cityModel)
        {
            _cities.InsertOne(cityModel);
            return GetByNameUa(cityModel.NameUa);
        }

        public void Update(string id, CityModel cityModel)
        {
            var users = _users.Find(user => true).ToList();
            var events = _events.Find(e => true).ToList();

            foreach (var user in users)
            {
                if (user.City.Id == id)
                {
                    user.City = cityModel;
                    _users.ReplaceOne(u => u.Id == user.Id, user);
                }
            }

            foreach (var Event in events)
            {
                if (Event.Location.Id == id)
                {
                    Event.Location = cityModel;
                    _events.ReplaceOne(e => e.Id == Event.Id, Event);
                }
            }
            _cities.ReplaceOne(city => city.Id == id, cityModel);
        }

        public void Delete(string id)
        {
            _cities.DeleteOne(city => city.Id == id);
        }
    }
}