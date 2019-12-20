using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Newtonsoft.Json;
using StackExchange.Redis;
using YourCityEventsApi.Model;
using YourCityEventsApi.Security;

namespace YourCityEventsApi.Services
{
    public class IdentityService
    {
        
        private IMongoCollection<BackendUserModel> _users;
        private IDatabase _redisUsersDatabase;
        private readonly string _jwtSecret;

        private readonly ConvertModelsService _convertModelsService;

            public IdentityService(IJwtSettings jwtSettings,IMongoSettings settings
            ,ConvertModelsService convertModelsService)
        {
            _jwtSecret = jwtSettings.Secret;
            
            var client=new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<BackendUserModel>("Users");
            
            var redis = RedisSettings.GetConnectionMultiplexer();
            _redisUsersDatabase = redis.GetDatabase(0);

            _convertModelsService = convertModelsService;
        }

        private AuthenticationResult GenerateAuthenticationResult(UserModel newUser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub,newUser.Email),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email,newUser.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key)
                    ,SecurityAlgorithms.HmacSha256Signature )
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token)
            };
        }
        public AuthenticationResult Register(string email,string password,string firstName,string lastName
        ,CityModel city)
        {
            var existing = _users.Find(u => u.Email == email).FirstOrDefault();

            if (existing != null)
            {
                return new AuthenticationResult
                {
                    Errors = new []{"User with this username already exists"}
                };
            }

            var newUser = new BackendUserModel(null,email,password,firstName,lastName,city);

            _users.InsertOne(newUser);
            var user = _users.Find(u => u.Email == newUser.Email).FirstOrDefault();
            _redisUsersDatabase.StringSet(user.Id, JsonConvert.SerializeObject(user)
            ,new TimeSpan(0,1,59,59));
            
            return GenerateAuthenticationResult(_convertModelsService.GetUserModel(newUser));
        }

        public AuthenticationResult Login(string email, string password)
        {
            var user = _users.Find(u => u.Email == email).FirstOrDefault();

            if (user == null)
            {
                return new AuthenticationResult
                {
                    Errors = new []{"User with this username does not exists"}
                };
            }

            if (user.Password != password)
            {
                return new AuthenticationResult
                {
                    Errors = new[] {"Incorrect password"}
                };
            }
            
            return GenerateAuthenticationResult(_convertModelsService.GetUserModel(user));
        }
    }
}