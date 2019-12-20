using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Driver;
using YourCityEventsApi.Model;
using System.Drawing;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace YourCityEventsApi.Services
{
    public class UserService
    {
        private IMongoCollection<BackendUserModel> _users;
        private IMongoCollection<EventModel> _events;
        private IDatabase _redisUsersDatabase;
        private IDatabase _redisEventsDatabase;
        private IEnumerable<RedisKey> _keys;
        private readonly TimeSpan ttl = new TimeSpan(0, 1, 59, 59);
        private readonly IHostingEnvironment _hostingEnvironment;

        private readonly ConvertModelsService _convertModelsService;
        
        public UserService(IMongoSettings mongoSettings,IHostingEnvironment hostingEnvironment
        ,ConvertModelsService convertModelsService)
        {
            var client=new MongoClient(mongoSettings.ConnectionString);
            var database = client.GetDatabase(mongoSettings.DatabaseName);
            _users = database.GetCollection<BackendUserModel>("Users");

            _events = database.GetCollection<EventModel>("Events");
            
            var redis = RedisSettings.GetConnectionMultiplexer();
            _redisUsersDatabase = redis.GetDatabase(0);
            _redisEventsDatabase = redis.GetDatabase(1);
            _keys = redis.GetServer(_redisUsersDatabase.Multiplexer.GetEndPoints().First()).Keys();
            
            _hostingEnvironment = hostingEnvironment;

            _convertModelsService = convertModelsService;
        }

        public List<UserModel> GetAll()
        {
            var allUsers = new List<UserModel>();
            foreach (var key in _keys)
            {
                Console.WriteLine("!!!!!!!!!!!!!!!");
                allUsers.Add(_convertModelsService.GetUserModel(JsonConvert.DeserializeObject<BackendUserModel>(_redisUsersDatabase.StringGet(key))));
            }

            return allUsers;
        }

        public UserModel Get(string token)
        {
            foreach (var key in _keys)
            {
                var user = JsonConvert.DeserializeObject<BackendUserModel>(_redisUsersDatabase.StringGet(key));
                if (user.Token == token)
                {
                    return _convertModelsService.GetUserModel(user);
                }
            }

            return null;
        }

        public UserModel GetById(string id)
        {
            foreach (var key in _keys)
            {
                var user = JsonConvert.DeserializeObject<BackendUserModel>(_redisUsersDatabase.StringGet(key));
                if (user.Id == id)
                {
                    return _convertModelsService.GetUserModel(user);
                }
            }

            return null;
        }

        public UserModel GetByEmail(string email)
        {
            foreach (var key in _keys)
            {
                var user = JsonConvert.DeserializeObject<BackendUserModel>(_redisUsersDatabase.StringGet(key));
                if (user.Email == email)
                {
                    return _convertModelsService.GetUserModel(user);
                }
            }

            return null;
        }

        public List<BackendEventModel> GetHostingEvents(string id)
        {
            var user = GetById(id);
            return user.HostingEvents.ToList();
        }
        
        public List<BackendEventModel> GetVisitingEvents(string id)
        {
            var user = GetById(id);
            return user.VisitingEvents.ToList();;
        }
        
        public void AddUserToken(string email,string token)
        {
            var user = _users.Find(u=>u.Email==email).FirstOrDefault();
            user.Token = token;
            _users.ReplaceOne(u => u.Email == email, user);
            _redisUsersDatabase.StringSet(user.Id, JsonConvert.SerializeObject(user), ttl);
        }
        
        public UserModel Create(BackendUserModel userModel)
        {
            _users.InsertOne(userModel);
            var user = _users.Find(u => u.Email == userModel.Email).FirstOrDefault();
            _redisUsersDatabase.StringSet(user.Id, JsonConvert.SerializeObject(user),ttl);
            return _convertModelsService.GetUserModel(user);
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

            _users.ReplaceOne(user => user.Id == id, _convertModelsService.GetBackendUserModel(userModel));
            _redisUsersDatabase.StringSet(id,JsonConvert.SerializeObject(userModel),ttl);
        }

        public string UploadImage(string token, UploadImageModel imageModel)
        {
            var user = Get(token);
            var wwwrootPath = _hostingEnvironment.WebRootPath;
            var directoryPath ="/users/"+user.Id+".jpg";
            var memoryStream = new MemoryStream(imageModel.Array);
            var image = Image.FromStream(memoryStream);
            File.Delete(wwwrootPath+directoryPath);
            image.Save(wwwrootPath + directoryPath);
            var finalPath="https://yourcityevents.azurewebsites.net"+directoryPath;
            user.ImageUrl = finalPath;
            _users.ReplaceOne(u => u.Id == user.Id, _convertModelsService.GetBackendUserModel(user));
            _redisUsersDatabase.StringSet(user.Id,JsonConvert.SerializeObject(
                _convertModelsService.GetBackendUserModel(user)),ttl);
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
            var user = _convertModelsService.GetBackendUserModel(Get(token));

            if (user.Password == password)
            {
                user.Email = newEmail;
                Update(user.Id,_convertModelsService.GetUserModel(user));
                return true;
            }

            return false;
        }

        public bool ChangePassword(string token,string password, string newPassword)
        {
            var user = _convertModelsService.GetBackendUserModel(Get(token));
            if (user.Password == password)
            {
                user.Password=newPassword;
                Update(user.Id,_convertModelsService.GetUserModel(user));
                return true;
            }

            return false;
        }

        /*public UserModel GetUserModel(BackendUserModel backendUserModel)
        {
            var userModel = UserModel.ConvertToUserModel(backendUserModel);
            
            var hostingEvents = new List<EventModel>();
            var visitingEvents = new List<EventModel>();

            if (backendUserModel.HostingEvents != null)
            {
                foreach (var id in backendUserModel.HostingEvents)
                {
                    hostingEvents.Add(_events.Find(e => e.Id == id).FirstOrDefault());
                }
            }

            if (backendUserModel.VisitingEvents != null)
            {
                foreach (var id in backendUserModel.VisitingEvents)
                {
                    visitingEvents.Add(_events.Find(e => e.Id == id).FirstOrDefault());
                }
            }

            userModel.HostingEvents = hostingEvents.ToArray();
            userModel.VisitingEvents = visitingEvents.ToArray();
            
            return userModel;
        }

        public BackendUserModel GetBackendUserModel(UserModel userModel)
        {
            var backendUserModel = BackendUserModel.ConvertToBackendUserModel(userModel);
            
            backendUserModel.Password = _users.Find(u => u.Id == backendUserModel.Id).FirstOrDefault().Password;
            backendUserModel.Token = _users.Find(u => u.Id == backendUserModel.Id).FirstOrDefault().Token;

            var hostingEvents = new List<string>();
            var visitingEvents = new List<string>();

            if (userModel.HostingEvents != null)
            {
                foreach (var Event in userModel.HostingEvents)
                {
                    hostingEvents.Add(Event.Id);
                }
            }

            if (userModel.VisitingEvents != null)
            {
                foreach (var Event in userModel.VisitingEvents)
                {
                    visitingEvents.Add(Event.Id);
                }
            }

            backendUserModel.HostingEvents = hostingEvents.ToArray();
            backendUserModel.VisitingEvents = visitingEvents.ToArray();

            return backendUserModel;
        }*/
    }
}