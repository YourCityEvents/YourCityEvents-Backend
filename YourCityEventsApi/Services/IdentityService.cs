using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using YourCityEventsApi.Model;
using YourCityEventsApi.Security;

namespace YourCityEventsApi.Services
{
    public class IdentityService
    {

        private readonly UserService _userService;
        private readonly string _jwtSecret;

            public IdentityService(IJwtSettings jwtSettings,UserService userService)
        {
            _userService=userService;
            _jwtSecret = jwtSettings.Secret;
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
            var existing = _userService.GetByEmail(email);

            if (existing != null)
            {
                return new AuthenticationResult
                {
                    Errors = new []{"User with this username already exists"}
                };
            }

            var newUser = new UserModel(null,email,password,firstName,lastName,city);

            var createdUser = _userService.Create(newUser);
            return GenerateAuthenticationResult(createdUser);
        }

        public AuthenticationResult Login(string email, string password)
        {
            var user = _userService.GetByEmail(email);

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
            
            return GenerateAuthenticationResult(user);
        }
    }
}