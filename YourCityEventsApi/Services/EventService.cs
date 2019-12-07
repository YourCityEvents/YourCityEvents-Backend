using System;
using MongoDB.Driver;
using YourCityEventsApi.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.AspNetCore.Hosting;


namespace YourCityEventsApi.Services
{
    public class EventService
    {
        private IMongoCollection<EventModel> _events;
        private IMongoCollection<UserModel> _users;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly UserService _userService;

        public EventService(IDatabaseSettings settings, IHostingEnvironment hostingEnvironment
            , UserService userService)
        {
            var client=new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _events = database.GetCollection<EventModel>("Events");
            _users = database.GetCollection<UserModel>("Users");
            _hostingEnvironment = hostingEnvironment;
            _userService = userService;
        }

        public List<EventModel> GetAll()
        {
            return _events.Find(e => true).ToList();
        }
        
        public List<EventModel> GetAllByCurrentDate()
        {
            return _events.Find(e => e.Date.CompareTo(DateTime.Now)>0).ToList();
        }

        public List<EventModel> GetByCity(CityModel cityModel)
        {
            return _events.Find(e => e.Location == cityModel&&e.Date
                                         .CompareTo(DateTime.Now)>0).ToList();
        }

        public List<EventModel> GetByToken(string token)
        {
            var city = _userService.Get(token).City;
            return _events.Find(e => e.Location == city && e.Date
                                         .CompareTo(DateTime.Now) > 0).ToList();
        }

        public EventModel Get(string id)
        {
            return _events.Find(e => e.Id == id).FirstOrDefault();
        }

        public EventModel GetByTitle(string title)
        {
            return _events.Find(e => e.Title == title).FirstOrDefault();
        }

        public List<UserModel> GetVisitors(string id)
        {
            return Get(id).Visitors.ToList();
        }

        private string UploadImage(string array, string eventId)
        {
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
            return createdEvent;
        }

        public void Update(string id, EventModel eventModel)
        {
            var users = _users.Find(user => true).ToList();
            foreach (var user in users)
            {
                if (user.HostingEvents != null)
                {
                    for (int i = 0; i < user.HostingEvents.Length; i++)
                    {
                        if (user.HostingEvents[i].Id == id)
                        {
                            user.HostingEvents[i] = eventModel;
                            _users.ReplaceOne(u => u.Id == user.Id, user);
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
                            _users.ReplaceOne(u => u.Id == user.Id, user);
                        }
                    }
                }
            }

            _events.ReplaceOne(Event => Event.Id == id, eventModel);
        }

        public void Delete(string id)
        {
            var users = _users.Find(user => true).ToList();
            foreach (var user in users)
            {
                user.HostingEvents = user.HostingEvents.Where(e => e.Id != id).ToArray();
                user.VisitingEvents = user.VisitingEvents.Where(e => e.Id != id).ToArray();
                _users.ReplaceOne(u => u.Id == user.Id, user);
            }

            _events.DeleteOne(Event => Event.Id == id);
        }
    }
}