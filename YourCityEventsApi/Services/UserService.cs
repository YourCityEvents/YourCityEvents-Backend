using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Driver;
using YourCityEventsApi.Model;
using System.Drawing;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace YourCityEventsApi.Services
{
    public class UserService
    {
        private IMongoCollection<UserModel> _users;
        private IMongoCollection<EventModel> _events;
        private IDatabase _redisUsersDatabase;
        private IDatabase _redisEventsDatabase;
        private IServer _server;
        private IEnumerable<RedisKey> _keys;
        private readonly TimeSpan ttl = new TimeSpan(0, 1, 59, 0);
        private readonly IHostingEnvironment _hostingEnvironment;

        public UserService(IMongoSettings mongoSettings,IHostingEnvironment hostingEnvironment)
        {
            var client=new MongoClient(mongoSettings.ConnectionString);
            var database = client.GetDatabase(mongoSettings.DatabaseName);
            _users = database.GetCollection<UserModel>("Users");

            _events = database.GetCollection<EventModel>("Events");
            
            var redis = RedisSettings.GetConnectionMultiplexer();
            _redisUsersDatabase = redis.GetDatabase(0);
            _redisEventsDatabase = redis.GetDatabase(1);
            _server = redis.GetServer(_redisUsersDatabase.Multiplexer.GetEndPoints().First());
            _keys = _server.Keys();
            
            _hostingEnvironment = hostingEnvironment;
        }

        public List<UserModel> GetAll()
        {
            var allUsers = new List<UserModel>();
            foreach (var key in _keys)
            {
                allUsers.Add(JsonConvert.DeserializeObject<UserModel>(_redisUsersDatabase.StringGet(key)));
            }

            return allUsers;
        }

        public UserModel Get(string token)
        {
            foreach (var key in _keys)
            {
                var user = JsonConvert.DeserializeObject<UserModel>(_redisUsersDatabase.StringGet(key));
                if (user.Token == token)
                {
                    return user;
                }
            }

            return null;
        }

        public UserModel GetById(string id)
        {
            foreach (var key in _keys)
            {
                var user = JsonConvert.DeserializeObject<UserModel>(_redisUsersDatabase.StringGet(key));
                if (user.Id == id)
                {
                    return user;
                }
            }

            return null;
        }

        public UserModel GetByEmail(string email)
        {
            foreach (var key in _keys)
            {
                var user = JsonConvert.DeserializeObject<UserModel>(_redisUsersDatabase.StringGet(key));
                if (user.Email == email)
                {
                    return user;
                }
            }

            return null;
        }

        public List<EventModel> GetHostingEvents(string id)
        {
            var user = GetById(id);
            return user.HostingEvents.ToList();
        }
        
        public List<EventModel> GetVisitingEvents(string id)
        {
            var user = GetById(id);
            return user.VisitingEvents.ToList();;
        }
        
        public void AddUserToken(string email,string token)
        {
            var user = GetByEmail(email);
            user.Token = token;
            _users.ReplaceOne(u => u.Email == email, user);
        }
        
        public UserModel Create(UserModel userModel)
        {
            _users.InsertOne(userModel);
            var user = GetByEmail(userModel.Email);
            _redisUsersDatabase.StringSet(user.Id, JsonConvert.SerializeObject(user),ttl);
            return user;
        }

        public void Update(string id, UserModel userModel)
        {
            var events = _events.Find(e=>true).ToList().ToArray();

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
                
                _events.ReplaceOne(events[i].Id, events[i]);
                _redisEventsDatabase.StringSet(events[i].Id, JsonConvert.SerializeObject(events[i]), ttl);
            }

            _users.ReplaceOne(user => user.Id == id, userModel);
            _redisUsersDatabase.StringSet(id,JsonConvert.SerializeObject(userModel),ttl);
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
            _redisUsersDatabase.StringSet(user.Id,JsonConvert.SerializeObject(user),ttl);
            return finalPath;
        }

        public void Delete(string id)
        {
            var events = _events.Find(e => true).ToList();
            foreach (var e in events)
            {
                if (e.Owner.Id == id)
                {
                    _events.DeleteOne(e.Id);
                    _redisEventsDatabase.KeyDelete(e.Id);
                    break;
                }

                e.Visitors = e.Visitors.Where(visitor => visitor.Id != id).ToArray();
                _events.ReplaceOne(e.Id, e);
                _redisEventsDatabase.StringSet(e.Id, JsonConvert.SerializeObject(e), ttl);
            }
            _users.DeleteOne(user => user.Id == id);
            _redisUsersDatabase.KeyDelete(id);
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