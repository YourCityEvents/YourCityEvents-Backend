using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using YourCityEventsApi.Model;


namespace YourCityEventsApi.Services
{
    public class UserService
    {
        private IMongoCollection<UserModel> _users;
        private IMongoCollection<EventModel> _events;
        
        public UserService(IDatabaseSettings settings)
        {
            var client=new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<UserModel>("Users");
            _events = database.GetCollection<EventModel>("Events");
        }

        public List<UserModel> Get()
        {
            return _users.Find(user => true).ToList();
        }

        public UserModel Get(string token) =>
            _users.Find(user => user.Token == token).FirstOrDefault();

        public UserModel GetById(string id) =>
            _users.Find(user => user.Id == id).FirstOrDefault();

        public UserModel GetByEmail(string email)
        {
            return _users.Find(user => user.Email==email)
                .FirstOrDefault();
        }

        public List<EventModel> GetHostingEvents(string id)
        {
            List<EventModel> eventList = new List<EventModel>();
            var user = GetById(id);
            foreach (var event_id in user.HostingEvents)
            {
                eventList.Add(_events.Find(e=>e.Id==event_id).FirstOrDefault());
            }
            return eventList;
        }
        
        public List<EventModel> GetVisitingEvents(string id)
        {
            List<EventModel> eventList = new List<EventModel>();
            var user = GetById(id);
            foreach (var eventId in user.VisitingEvents)
            {
                eventList.Add(_events.Find(e=>e.Id==eventId).FirstOrDefault());
            }
            return eventList;
        }
        
        public void AddUserToken(string email,string token)
        {
            var user = GetByEmail(email);
            user.Token = token;
            Update(user.Id,user);
        }
        
        public UserModel Create(UserModel userModel)
        {
            _users.InsertOne(userModel);
            return GetByEmail(userModel.Email);
        }

        public void Update(string id, UserModel userModel) =>
            _users.ReplaceOne(user => user.Id == id, userModel);
        
        public void Delete(string id) =>
            _users.DeleteOne(user => user.Id == id);

        public bool ChangeEmail(string token,string password, string newEmail)
        {
            UserModel user = Get(token);

            if (user.Password == password)
            {
                user.Email = newEmail;
                Update(user.Id,user);
                return true;
            }

            return false;
        }

        public bool ChangePassword(string token,string password, string newPassword)
        {
            UserModel user = Get(token);
            if (user.Password == password)
            {
                user.Password=newPassword;
                Update(user.Id,user);
                return true;
            }

            return false;
        }
    }
}