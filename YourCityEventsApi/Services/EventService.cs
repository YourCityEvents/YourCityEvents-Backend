using System;
using MongoDB.Driver;
using YourCityEventsApi.Model;
using System.Collections.Generic;
using System.Linq;
using Microsoft.IdentityModel.Tokens;

namespace YourCityEventsApi.Services
{
    public class EventService
    {
        private IMongoCollection<EventModel> _events;
        private IMongoCollection<UserModel> _users;

        public EventService(IDatabaseSettings settings)
        {
            var client=new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _events = database.GetCollection<EventModel>("Events");
            _users = database.GetCollection<UserModel>("Users");
            
        }

        public List<EventModel> Get()
        {
            return _events.Find(Event => true).ToList();
        }
            


        public EventModel Get(string id) =>
            _events.Find(Event => Event.Id == id).FirstOrDefault();

        public EventModel GetByTitle(string title) =>
            _events.Find(Event => Event.Title == title).FirstOrDefault();

        public List<UserModel> GetVisitors(string id)
        {
            return Get(id).Visitors.ToList();
        }

        public EventModel Create(EventModel eventModel)
        {
            _events.InsertOne(eventModel);
            return GetByTitle(eventModel.Title);
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