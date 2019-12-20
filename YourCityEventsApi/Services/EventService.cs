using System;
using MongoDB.Driver;
using YourCityEventsApi.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.AspNetCore.Hosting;
using MongoDB.Bson;
using Newtonsoft.Json;
using StackExchange.Redis;


namespace YourCityEventsApi.Services
{
    public class EventService
    {
        private IMongoCollection<BackendEventModel> _events;
        private IMongoCollection<BackendUserModel> _users;
        private IDatabase _redisEventsDatabase;
        private IDatabase _redisUsersDatabase;
        private IEnumerable<RedisKey> _keys;
        private readonly TimeSpan ttl = new TimeSpan(0, 1, 59, 59);
        private readonly IHostingEnvironment _hostingEnvironment;

        private readonly ConvertModelsService _convertModelsService;
        
        public EventService(IMongoSettings settings, IHostingEnvironment hostingEnvironment
        ,ConvertModelsService convertModelsService)
        {
            var client=new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _events = database.GetCollection<BackendEventModel>("Events");
            _hostingEnvironment = hostingEnvironment;

            _users = database.GetCollection<BackendUserModel>("Users");
            
            var redis = RedisSettings.GetConnectionMultiplexer();
            _redisUsersDatabase = redis.GetDatabase(0);
            _redisEventsDatabase = redis.GetDatabase(1);
            _keys = redis.GetServer(_redisEventsDatabase.Multiplexer.GetEndPoints().First()).Keys(1);

            _convertModelsService = convertModelsService;
        }

        public List<EventModel> GetAll()
        {
            var allEvents = new List<EventModel>();
            foreach (var key in _keys)
            {
                allEvents.Add(_convertModelsService.GetEventModel(
                    JsonConvert.DeserializeObject<BackendEventModel>(_redisEventsDatabase.StringGet(key))));
            }
            return allEvents;
        }
        
        public List<EventModel> GetAllByCurrentDate()
        {
            var allEvents = new List<EventModel>();
            foreach (var key in _keys)
            {
                var Event = JsonConvert.DeserializeObject<BackendEventModel>(_redisEventsDatabase.StringGet(key));
                if (DateTime.ParseExact(Event.Date,"dd/MM/yyyy HH:mm",null).CompareTo(DateTime.Now) > 0)
                {
                    allEvents.Add(_convertModelsService.GetEventModel(Event));
                }
            }

            return allEvents;
        }

        public List<EventModel> GetByCity(CityModel cityModel)
        {
            var allEvents = new List<EventModel>();
            foreach (var key in _keys)
            {
                var Event = JsonConvert.DeserializeObject<BackendEventModel>(_redisEventsDatabase.StringGet(key));
                if (Event.Location.Id == cityModel.Id && DateTime.ParseExact(Event.Date,"dd/MM/yyyy HH:mm",null).CompareTo(DateTime.Now) > 0)
                {
                    allEvents.Add(_convertModelsService.GetEventModel(Event));
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
                var Event = JsonConvert.DeserializeObject<BackendEventModel>(_redisEventsDatabase.StringGet(key));
                if (Event.Location.Id == city.Id&&DateTime.ParseExact(Event.Date,"dd/MM/yyyy HH:mm",null).CompareTo(DateTime.Now)>0)
                {
                    allEvents.Add(_convertModelsService.GetEventModel(Event));
                }
            }

            return allEvents;
        }

        public EventModel Get(string id)
        {
            foreach (var key in _keys)
            {
                var Event=JsonConvert.DeserializeObject<BackendEventModel>(_redisEventsDatabase.StringGet(key));
                if (Event.Id == id)
                {
                    return _convertModelsService.GetEventModel(Event);
                }
            }

            return null;
        }

        public EventModel GetByTitle(string title)
        {
            foreach (var key in _keys)
            {
                var Event=JsonConvert.DeserializeObject<BackendEventModel>(_redisEventsDatabase.StringGet(key));
                if (Event.Title == title)
                {
                    return _convertModelsService.GetEventModel(Event);
                }
            }

            return null;
        }

        public List<UserModel> GetVisitors(string id)
        {
            return Get(id).Visitors.ToList();
        }

        private string UploadImage(string eventId,byte[] array)
        {
            var wwwrootPath = _hostingEnvironment.WebRootPath;
            var directoryPath = "/events/" + eventId + ".jpg";
            var memoryStream = new MemoryStream(array);
            var image = Image.FromStream(memoryStream);
            File.Delete(wwwrootPath+directoryPath);
            image.Save(wwwrootPath+directoryPath);
            return "https://yourcityevents.azurewebsites.net" + directoryPath;
        }

        public EventModel Create(CreateEventRequest eventModel,string ownerToken)
        {
            var owner = _users.Find(u => u.Token == ownerToken).FirstOrDefault();
            var createdEvent=new BackendEventModel(null,eventModel.Title,owner.City,eventModel.DetailLocation
            ,eventModel.Description,owner.Id,eventModel.Date,eventModel.Price);
            _events.InsertOne(createdEvent);
            _redisEventsDatabase.StringSet(createdEvent.Id, JsonConvert.SerializeObject(createdEvent), ttl);
            createdEvent = _convertModelsService.GetBackendEventModel(GetByTitle(eventModel.Title));

            if (eventModel.ImageArray != null)
            {
                var imageUrl = UploadImage(createdEvent.Id, eventModel.ImageArray);
                createdEvent.ImageUrl = imageUrl;
                _events.ReplaceOne(e => e.Id == createdEvent.Id, createdEvent);
                _redisEventsDatabase.StringSet(createdEvent.Id, JsonConvert.SerializeObject(createdEvent), ttl);
            }

            return _convertModelsService.GetEventModel(createdEvent);
        }

        /*public void Update(string id,EventModel eventModel)
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
                            Console.WriteLine(user.Id);
                            Console.WriteLine(user.HostingEvents[i].Id);
                            Console.WriteLine(eventModel.Id);
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
        }*/

        public void SubscribeOnEvent(string id, string token)
        {
            var user = _users.Find(u => u.Token == token).FirstOrDefault();
            var Event = _events.Find(e => e.Id == id).FirstOrDefault();
            Event.Visitors.ToList().Add(user.Id);
            _events.ReplaceOne(e => e.Id == id, Event);
            _redisEventsDatabase.StringSet(id, JsonConvert.SerializeObject(Event), ttl);
        }
        
        public void Delete(string id)
        {
            var users = _users.Find(u => true).ToList();
            foreach (var user in users)
            {
                user.HostingEvents = user.HostingEvents.Where(i => i != id).ToArray();
                user.VisitingEvents = user.VisitingEvents.Where(i => i != id).ToArray();
                _users.ReplaceOne(user.Id, user);
                _redisUsersDatabase.StringSet(user.Id, JsonConvert.SerializeObject(user), ttl);
            }

            _events.DeleteOne(Event => Event.Id == id);
            _redisEventsDatabase.KeyDelete(id);
        }
    }
}