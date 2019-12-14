using System;
using MongoDB.Driver;
using YourCityEventsApi.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using StackExchange.Redis;


namespace YourCityEventsApi.Services
{
    public class EventService
    {
        private IMongoCollection<EventModel> _events;
        private IMongoCollection<UserModel> _users;
        private IDatabase _redisEventsDatabase;
        private IDatabase _redisUsersDatabase;
        private IServer _server;
        private IEnumerable<RedisKey> _keys;
        private readonly TimeSpan ttl = new TimeSpan(0, 1, 59, 59);
        private readonly IHostingEnvironment _hostingEnvironment;

        public EventService(IMongoSettings settings, IHostingEnvironment hostingEnvironment)
        {
            var client=new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _events = database.GetCollection<EventModel>("Events");
            _hostingEnvironment = hostingEnvironment;

            _users = database.GetCollection<UserModel>("Users");
            
            var redis = RedisSettings.GetConnectionMultiplexer();
            _redisUsersDatabase = redis.GetDatabase(0);
            _redisEventsDatabase = redis.GetDatabase(1);
            _server = redis.GetServer(_redisEventsDatabase.Multiplexer.GetEndPoints().First());
            _keys = _server.Keys(1);
        }

        public List<EventModel> GetAll()
        {
            var allEvents = new List<EventModel>();
            foreach (var key in _keys)
            {
                allEvents.Add(JsonConvert.DeserializeObject<EventModel>(_redisEventsDatabase.StringGet(key)));
            }
            return allEvents;
        }
        
        public List<EventModel> GetAllByCurrentDate()
        {
            var allEvents = new List<EventModel>();
            foreach (var key in _keys)
            {
                var Event = JsonConvert.DeserializeObject<EventModel>(_redisEventsDatabase.StringGet(key));
                if (Event.Date.CompareTo(DateTime.Now) > 0)
                {
                    allEvents.Add(Event);
                }
            }

            return allEvents;
        }

        public List<EventModel> GetByCity(CityModel cityModel)
        {
            var allEvents = new List<EventModel>();
            foreach (var key in _keys)
            {
                var Event = JsonConvert.DeserializeObject<EventModel>(_redisEventsDatabase.StringGet(key));
                if (Event.Location == cityModel && Event.Date.CompareTo(DateTime.Now) > 0)
                {
                    allEvents.Add(Event);
                }
            }

            return allEvents;
        }

        public List<EventModel> GetByToken(string token)
        {
            var allEvents = new List<EventModel>();
            var city = _users.Find(u => u.Token == token).FirstOrDefault().City;
            foreach (var key in _keys)
            {
                var Event = JsonConvert.DeserializeObject<EventModel>(_redisEventsDatabase.StringGet(key));
                if (Event.Location == city&&Event.Date.CompareTo(DateTime.Now)>0)
                {
                    allEvents.Add(Event);
                }
            }

            return allEvents;
        }

        public EventModel Get(string id)
        {
            foreach (var key in _keys)
            {
                var Event=JsonConvert.DeserializeObject<EventModel>(_redisEventsDatabase.StringGet(key));
                if (Event.Id == id)
                {
                    return Event;
                }
            }

            return null;
        }

        public EventModel GetByTitle(string title)
        {
            foreach (var key in _keys)
            {
                var Event=JsonConvert.DeserializeObject<EventModel>(_redisEventsDatabase.StringGet(key));
                if (Event.Title == title)
                {
                    return Event;
                }
            }

            return null;
        }

        public List<UserModel> GetVisitors(string id)
        {
            return Get(id).Visitors.ToList();
        }

        private string UploadImage(string eventId,string array)
        {
            var Event = Get(eventId);
            var wwwrootPath = _hostingEnvironment.WebRootPath;
            var directoryPath = "/events/" + eventId + ".jpg";
            var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(array));
            var image = Image.FromStream(memoryStream);
            image.Save(wwwrootPath+directoryPath);
            return "https://yourcityevents.azurewebsites.net" + directoryPath;
        }

        public EventModel Create(EventModel eventModel)
        {
            _events.InsertOne(eventModel);
            var createdEvent = GetByTitle(eventModel.Title);
            var imageUrl=UploadImage(createdEvent.ImageUrl,createdEvent.Id);
            createdEvent.ImageUrl = imageUrl;
            _events.ReplaceOne(e => e.Id == createdEvent.Id, createdEvent);
            _redisEventsDatabase.StringSet(createdEvent.Id, JsonConvert.SerializeObject(createdEvent), ttl);
            return createdEvent;
        }

        public void Update(string id, EventModel eventModel)
        {
            var users = _users.Find(u => true).ToList();
            foreach (var user in users)
            {
                if (user.HostingEvents != null)
                {
                    for (int i = 0; i < user.HostingEvents.Length; i++)
                    {
                        if (user.HostingEvents[i].Id == id)
                        {
                            user.HostingEvents[i] = eventModel;
                            _users.ReplaceOne(user.Id, user);
                            _redisUsersDatabase.StringSet(user.Id, JsonConvert.SerializeObject(user), ttl);
                        }
                    }
                }

                if (user.VisitingEvents != null)
                {
                    for (int i = 0; i < user.VisitingEvents.Length; i++)
                    {
                        if (user.VisitingEvents[i].Id == id)
                        {
                            user.VisitingEvents[i] = eventModel;
                            _users.ReplaceOne(user.Id, user);
                            _redisUsersDatabase.StringSet(user.Id, JsonConvert.SerializeObject(user), ttl);
                        }
                    }
                }
            }

            _events.ReplaceOne(Event => Event.Id == id, eventModel);
            _redisEventsDatabase.StringSet(id, JsonConvert.SerializeObject(eventModel), ttl);
        }

        public void Delete(string id)
        {
            var users = _users.Find(u => true).ToList();
            foreach (var user in users)
            {
                user.HostingEvents = user.HostingEvents.Where(e => e.Id != id).ToArray();
                user.VisitingEvents = user.VisitingEvents.Where(e => e.Id != id).ToArray();
                _users.ReplaceOne(user.Id, user);
                _redisUsersDatabase.StringSet(user.Id, JsonConvert.SerializeObject(user), ttl);
            }

            _events.DeleteOne(Event => Event.Id == id);
            _redisEventsDatabase.KeyDelete(id);
        }
    }
}