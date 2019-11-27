using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Emit;
using YourCityEventsApi.Model;
using YourCityEventsApi.Services;

namespace YourCityEventsApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly EventService _eventService;
        private readonly UserService _userService;

        public EventController(EventService eventService,UserService userService)
        {
            _eventService = eventService;
            _userService = userService;;
        }

        [HttpGet("all")]
        public ActionResult<ResponseModel<List<EventModel>>> GetAll()
        {
            var eventList = _eventService.GetAll();
            
            return ResponseModel<List<EventModel>>.FormResponse("events", eventList
                ,  "Unable to get events");
        }

        [HttpGet("for_all_cities")]
        public ActionResult<ResponseModel<List<EventModel>>> GetAllByCurrentDate()
        {
            var eventList = _eventService.GetAllByCurrentDate();
            
            return ResponseModel<List<EventModel>>.FormResponse("events", eventList
                , "Unable to get events");
        }
        
        [HttpGet]
        public ActionResult<ResponseModel<List<EventModel>>> GetByCity([FromHeader] string Authorization)
        {
            string token = Authorization.Split()[1];
            var city = _userService.Get(token).City;
            var eventList = _eventService.GetByCity(city);

            return ResponseModel<List<EventModel>>.FormResponse("events", eventList, "Unable to get events");
        }

        [HttpGet("byCity")]
        public ActionResult<ResponseModel<List<EventModel>>> GetByCity(CityModel cityModel)
        {
            var eventList = _eventService.GetByCity(cityModel);
            
            return ResponseModel<List<EventModel>>.FormResponse("events", eventList, "Unable to get events");
        }

        [HttpGet("{id}")]
        public ActionResult<ResponseModel<EventModel>> Get(string id)
        {
            var Event = _eventService.Get(id);
            
            return ResponseModel<EventModel>.FormResponse("event", Event, "Event not found");
        }

        [HttpGet("{id}/visitors")]
        public ActionResult<ResponseModel<List<UserModel>>> GetVisitors(string id)
        {
            var userList = _eventService.GetVisitors(id);

            return ResponseModel<List<UserModel>>.FormResponse("users", userList, "Unable to get users");
        }

        [HttpPost]
        public ActionResult<ResponseModel<EventModel>> Create(EventModel eventModel)
        {
            var Event = _eventService.Create(eventModel);

            return ResponseModel<EventModel>.FormResponse("event", Event, "Unable to create event");
        }

        [HttpPut("{id}")]
        public ActionResult<ResponseModel<EventModel>> Update(string id, EventModel eventModel)
        {
            var Event = _eventService.Get(id);

            return ResponseModel<EventModel>.FormResponse("event", Event, "Unable to find event for updating");
        }

        [HttpDelete("{id}")]
        public ActionResult<ResponseModel<string>> Delete(string id)
        {
            var Event = _eventService.Get(id);
            if (Event != null)
            {
                _eventService.Delete(id);
                return new ResponseModel<string>(null);
            }
            
            return new ResponseModel<string>(null, false, new[] {"Unable to find event for deleting"});
        }
    }
}