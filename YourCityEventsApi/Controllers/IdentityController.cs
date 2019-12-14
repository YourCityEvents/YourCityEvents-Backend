using System.Linq;
using Microsoft.AspNetCore.Mvc;
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
                return new ResponseModel<string>(null,false,errors);
            }

            var authResponse = _identityService.Register(request.Email.ToLower(), request.Password, request.FirstName
                , request.LastName, request.City);

            if (!authResponse.Success)
            {
                return new ResponseModel<string>(null,false,authResponse.Errors);

            }
            
            _userService.AddUserToken(request.Email.ToLower(),authResponse.Token);
           
            return ResponseModel<string>.FormResponse("token",authResponse.Token,null);
        }

        [HttpPost("login")]
        public ActionResult<ResponseModel<string>> Login(UserLoginRequest request)
        {
            var authResponse = _identityService.Login(request.Email.ToLower(), request.Password);

            if (!authResponse.Success)
            {
                return new ResponseModel<string>(null, false, authResponse.Errors);

            }

            _userService.AddUserToken(request.Email.ToLower(), authResponse.Token);

            return ResponseModel<string>.FormResponse("token", authResponse.Token, null);

        }


    }
    
}