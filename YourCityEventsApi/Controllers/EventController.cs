using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
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

        public EventController(EventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Get all events for all cities
        /// </summary>
        /// <returns></returns>
        [HttpGet("all")]
        public ActionResult<ResponseModel<List<EventModel>>> GetAll()
        {
            var eventList = _eventService.GetAll();
            
            return ResponseModel<List<EventModel>>.FormResponse("events", eventList
                ,  "Unable to get events");
        }

        /// <summary>
        /// Get relevant events for all cities
        /// </summary>
        /// <returns></returns>
        [HttpGet("forAllCities")]
        public ActionResult<ResponseModel<List<EventModel>>> GetAllByCurrentDate()
        {
            var eventList = _eventService.GetAllByCurrentDate();
            
            return ResponseModel<List<EventModel>>.FormResponse("events", eventList
                , "Unable to get events");
        }
        
        /// <summary>
        /// Get relevant events for user's city
        /// </summary>
        /// <param name="Authorization"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult<ResponseModel<List<EventModel>>> GetByCity([FromHeader] string Authorization)
        {
            string token = Authorization.Split()[1];
            var eventList = _eventService.GetByToken(token);

            return ResponseModel<List<EventModel>>.FormResponse("events", eventList, "Unable to get events");
        }

        /// <summary>
        /// Get relevant events for specific city
        /// </summary>
        /// <param name="cityModel"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get visitors for specific event
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/visitors")]
        public ActionResult<ResponseModel<List<UserModel>>> GetVisitors(string id)
        {
            var userList = _eventService.GetVisitors(id);

            return ResponseModel<List<UserModel>>.FormResponse("users", userList, "Unable to get users");
        }

        [HttpPost]
        public ActionResult<ResponseModel<EventModel>> Create(CreateEventRequest eventModel,[FromHeader] string Authorization)
        {
            string token = Authorization.Split()[1];
            var Event = _eventService.Create(eventModel,token);

            return ResponseModel<EventModel>.FormResponse("event", Event, "Unable to create event");
        }

        /*[HttpPut("{id}")]
        public ActionResult<ResponseModel<string>> Update(string id,[FromBody]EventModel eventModel)
        {
            _eventService.Update(id,eventModel);

            return new ResponseModel<string>(null);
        }*/

        /// <summary>
        /// Subscribe on specific event
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Authorization"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public ActionResult<ResponseModel<string>> SubscribeOnEvent(string id, [FromHeader] string Authorization)
        {
            string token = Authorization.Split()[1];
            _eventService.SubscribeOnEvent(id, token);
            return new ResponseModel<string>(null);
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