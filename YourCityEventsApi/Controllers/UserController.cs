using System;
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
        public ActionResult<ResponseModel<List<UserModel>>> Get()
        {
            var userList= _userService.Get();
            var data=new Dictionary<string,List<UserModel>>();
            data.Add("users",userList);
            if (userList != null)
            {
                return new ResponseModel<List<UserModel>>(data);
            }

            return new ResponseModel<List<UserModel>>(null, false, new[] {"Unable to get models"});
        }

        [HttpGet]
        public ActionResult<ResponseModel<UserModel>> Get([FromHeader] string Authorization)
        {
            string token = Authorization.Split()[1];
            var user = _userService.Get(token);
            var data=new Dictionary<string,UserModel>();
            data.Add("user",user);
            if (user != null)
            {
                return new ResponseModel<UserModel>(data);
            }
            
            return new ResponseModel<UserModel>(null, false, new[] {"User not found"});

        }

        [HttpGet("{id}")]
        public ActionResult<ResponseModel<UserModel>> GetById(string id)
        {
            var user = _userService.GetById(id);
            var data=new Dictionary<string,UserModel>();
            data.Add("user",user);
            if (user != null)
            {
                return new ResponseModel<UserModel>(data);
            }
            
            return new ResponseModel<UserModel>(null, false, new[] {"User not found"});

        }

        [HttpGet("{id}/hostingEvents")]
        public ActionResult<ResponseModel<List<EventModel>>> GetHostingEvents(string id)
        {
            var eventList=  _userService.GetHostingEvents(id);
            var data=new Dictionary<string,List<EventModel>>();
            data.Add("events",eventList);
            if (eventList != null)
            {
                return new ResponseModel<List<EventModel>>(data);
            }
            
            return new ResponseModel<List<EventModel>>(null, false, new[] {"Unable to get events"});

        }

        [HttpGet("{id}/visitingEvents")]
        public ActionResult<ResponseModel<List<EventModel>>> GetVisitingEvents(string id)
        {
            var eventList=_userService.GetVisitingEvents(id);
            var data=new Dictionary<string,List<EventModel>>();
            data.Add("events",eventList);
            if (eventList != null)
            {
                return new ResponseModel<List<EventModel>>(data);
            }
            
            return new ResponseModel<List<EventModel>>(null, false, new[] {"Unable to get events"});
        }

        [HttpPost]
        public ActionResult<ResponseModel<UserModel>> Create(UserModel userModel)
        {
            var user= _userService.Create(userModel);
            var data=new Dictionary<string,UserModel>();
            data.Add("user",userModel);
            if (user != null)
            {
                return new ResponseModel<UserModel>(data);
            }
            
            return new ResponseModel<UserModel>(null, false, new[] {"Unable to create user"});
        }
        
        [HttpPut("{id}")]
        public ActionResult<ResponseModel<UserModel>> Update(string id,UserModel userModel)
        {
            var user = _userService.GetById(id);
            var data=new Dictionary<string,UserModel>();
            data.Add("user",userModel);
            if (user != null)
            {
                _userService.Update(id,userModel);
                return new ResponseModel<UserModel>(data);
            }
            
            return new ResponseModel<UserModel>(null, false, new[] {"Unable to find user for updating"});
        }

        [HttpPut("change_password")]
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
        
        [HttpPut("change_email")]
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

        [HttpPut("upload_image")]
        public ActionResult<ResponseModel<string>> UploadImage([FromHeader] string Authorization
        ,UploadImageModel imageModel)
        {
            string token = Authorization.Split()[1];
            string path = _userService.UploadImage(token, imageModel);
            var data=new Dictionary<string,string>();
            data.Add("path",path);
            
            if (path != null)
            {
                return new ResponseModel<string>(data);
            }

            return new ResponseModel<string>(null, false, new[] {"Unable to upload image"});
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