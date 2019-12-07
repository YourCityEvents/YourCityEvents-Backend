using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Driver;
using YourCityEventsApi.Model;
using System.Drawing;
using Microsoft.AspNetCore.Hosting;
using System.Linq;

namespace YourCityEventsApi.Services
{
    public class UserService
    {
        private IMongoCollection<UserModel> _users;
        private IMongoCollection<EventModel> _events;
        private readonly IHostingEnvironment _hostingEnvironment;
        
        public UserService(IDatabaseSettings settings,IHostingEnvironment hostingEnvironment)
        {
            var client=new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<UserModel>("Users");
            _events = database.GetCollection<EventModel>("Events");
            _hostingEnvironment = hostingEnvironment;
        }

        public List<UserModel> Get()
        {
            return _users.Find(user => true).ToList();
        }

        public UserModel Get(string token)
        {
            return _users.Find(user => user.Token == token).FirstOrDefault();
        }

        public UserModel GetById(string id)
        {
            return _users.Find(user => user.Id == id).FirstOrDefault();
        }

        public UserModel GetByEmail(string email)
        {
            return _users.Find(user => user.Email==email)
                .FirstOrDefault();
        }

        public List<EventModel> GetHostingEvents(string id)
        {
            List<EventModel> eventList = new List<EventModel>();
            var user = GetById(id);
            eventList = user.HostingEvents.ToList();
            return eventList;
        }
        
        public List<EventModel> GetVisitingEvents(string id)
        {
            List<EventModel> eventList = new List<EventModel>();
            var user = GetById(id);
            eventList = user.VisitingEvents.ToList();
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

        public void Update(string id, UserModel userModel)
        {
            var events = _events.Find(e => true).ToList().ToArray();

            for (int i = 0; i < events.Length; i++)
            {
                if (events[i].Owner.Id == id)
                {
                    events[i].Owner = userModel;
                }

                if (events[i].Visitors != null)
                {
                    for (int j = 0; j < events[i].Visitors.Length; j++)
                    {
                        if (events[i].Visitors[j].Id == id)
                        {
                            events[i].Visitors[j] = userModel;
                        }
                    }
                }
                
                _events.ReplaceOne(e => e.Id == events[i].Id, events[i]);
            }

            _users.ReplaceOne(user => user.Id == id, userModel);
        }

        public string UploadImage(string token, UploadImageModel imageModel)
        {
            var user = Get(token);
            var wwwrootPath = _hostingEnvironment.WebRootPath;
            var directoryPath ="/users/"+user.Id+".jpg";
            var memoryStream = new MemoryStream(imageModel.Array);
            var image = Image.FromStream(memoryStream);
            image.Save(wwwrootPath + directoryPath);
            var finalPath="https://yourcityevents.azurewebsites.net"+directoryPath;
            user.ImageUrl = finalPath;
            _users.ReplaceOne(u => u.Id == user.Id, user);
            return finalPath;
        }

        public void Delete(string id)
        {
            var events = _events.Find(e => true).ToList();
            foreach (var e in events)
            {
                if (e.Owner.Id == id)
                {
                    _events.DeleteOne(ev => ev.Id == e.Id);
                    break;
                }

                e.Visitors = e.Visitors.Where(visitor => visitor.Id != id).ToArray();
                _events.ReplaceOne(ev => ev.Id == e.Id, e);
            }
            _users.DeleteOne(user => user.Id == id);
        }

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