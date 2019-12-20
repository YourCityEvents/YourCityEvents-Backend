using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using YourCityEventsApi.Model;
using Microsoft.AspNetCore.Authorization;
using YourCityEventsApi.Services;

namespace YourCityEventsApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("all")]
        public ActionResult<ResponseModel<List<UserModel>>> GetAll()
        {
            var userList= _userService.GetAll();
            
            return ResponseModel<List<UserModel>>.FormResponse("users",userList,"Unable to get users");
        }

        /// <summary>
        /// Get user by token
        /// </summary>
        /// <param name="Authorization"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<ResponseModel<UserModel>> Get([FromHeader] string Authorization)
        {
            string token = Authorization.Split()[1];
            var user = _userService.Get(token);

            return ResponseModel<UserModel>.FormResponse("user",user, "User not found");
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public ActionResult<ResponseModel<UserModel>> GetById(string id)
        {
            var user = _userService.GetById(id);

            return ResponseModel<UserModel>.FormResponse("user",user,"User not found");
        }

        /// <summary>
        /// Get specific user's hosting events
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/hostingEvents")]
        public ActionResult<ResponseModel<List<BackendEventModel>>> GetHostingEvents(string id)
        {
            var eventList=  _userService.GetHostingEvents(id);

            return ResponseModel<List<BackendEventModel>>.FormResponse("events",eventList,"Unable to get events");
        }

        /// <summary>
        ///  Get specific user's visiting events
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/visitingEvents")]
        public ActionResult<ResponseModel<List<BackendEventModel>>> GetVisitingEvents(string id)
        {
            var eventList=_userService.GetVisitingEvents(id);

            return ResponseModel<List<BackendEventModel>>.FormResponse("events",eventList,"Unable to get events");
        }

        [HttpPost]
        public ActionResult<ResponseModel<UserModel>> Create(BackendUserModel userModel)
        {
            var user= _userService.Create(userModel);

            return ResponseModel<UserModel>.FormResponse("user",user,"Unable to create user");
        }
        
        [HttpPut("{id}")]
        public ActionResult<ResponseModel<string>> Update(string id,UserModel userModel)
        {
            _userService.Update(id,userModel);

            return new ResponseModel<string>(null);
        }

        [HttpPut("changePassword")]
        public ActionResult<ResponseModel<string>> ChangePassword([FromHeader] string Authorization
        ,ChangePasswordRequest request)
        {
            string token = Authorization.Split()[1];
            bool result = _userService.ChangePassword(token, request.Password,request.NewPassword);

            if (result)
            {
                return new ResponseModel<string>(null);
            }

            return new ResponseModel<string>(null,false,new []{"Wrong password"});
        }
        
        [HttpPut("changeEmail")]
        public ActionResult<ResponseModel<string>> ChangeEmail([FromHeader] string Authorization
        ,ChangeEmailRequest request)
        {
            string token = Authorization.Split()[1];
            bool result = _userService.ChangeEmail(token,request.Password,request.NewEmail); 
            if (result)
            {
                return new ResponseModel<string>(null);
            }

            return new ResponseModel<string>(null,false,new []{"Wrong password"});
        }

        [HttpPut("uploadImage")]
        public ActionResult<ResponseModel<string>> UploadImage([FromHeader] string Authorization
        ,UploadImageModel imageModel)
        {
            string token = Authorization.Split()[1];
            string path = _userService.UploadImage(token, imageModel);
            
            return ResponseModel<string>.FormResponse("path",path,"Unable to upload image");
        }
        
        [HttpDelete("{id}")]
        public ActionResult<ResponseModel<string>> Delete(string id)
        {
            var user = _userService.Get(id);
            if (user != null)
            {
                _userService.Delete(id);
                return new ResponseModel<string>(null);
            }

            return new ResponseModel<string>(null, false, new[] {"Unable to find user for deleting"});
        }
    }
}