using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using YourCityEventsApi.Model;
using YourCityEventsApi.Services;
using YourCityEventsApi.Security;

namespace YourCityEventsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly IdentityService _identityService;
        private readonly UserService _userService;

        public IdentityController(IdentityService identityService,UserService userService)
        {
            _identityService = identityService;
            _userService = userService;
        }

        [HttpPost("register")]
        public ActionResult<ResponseModel<string>> Register(UserRegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage));
                return new ResponseModel<string>(null,"false",errors);
            }

            var authResponse = _identityService.Register(request.Email, request.Password, request.FirstName
                , request.LastName, request.City);
            
            _userService.AddUserToken(request.Email,authResponse.Token);
            
            if (!authResponse.Success)
            {
                return new ResponseModel<string>(null,"false",authResponse.Errors);

            }
            
            var data=new Dictionary<string,string>();
            data.Add("token",authResponse.Token);
            
            return new ResponseModel<string>(data);
        }
        
        [HttpPost("login")]
        public ActionResult<ResponseModel<string>> Login(UserLoginRequest request)
        {
            var authResponse = _identityService.Login(request.Email, request.Password);

            _userService.AddUserToken(request.Email,authResponse.Token);
            
            if (!authResponse.Success)
            {
                return new ResponseModel<string>(null,"false",authResponse.Errors);

            }
            
            var data=new Dictionary<string,string>();
            data.Add("token",authResponse.Token);

            return new ResponseModel<string>(data);
        }
    }
    
}