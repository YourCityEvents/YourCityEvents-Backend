using System.Collections;
using Microsoft.AspNetCore.Mvc;
using YourCityEventsApi.Services;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Bson.Serialization.Attributes;
using YourCityEventsApi.Model;

namespace YourCityEventsApi.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CityController:ControllerBase
    {
        private readonly CityService _cityService;

        public CityController(CityService cityService)
        {
            _cityService = cityService;
        }

        [HttpGet]
        public ActionResult<ResponseModel<List<CityModel>>> Get()
        {
            var cityList = _cityService.Get();

            return ResponseModel<List<CityModel>>.FormResponse("cities", cityList, "Unable to get cities");
        }

        [HttpGet("{id}")]
        public ActionResult<ResponseModel<CityModel>> Get(string id)
        {
            var city = _cityService.Get(id);

            return ResponseModel<CityModel>.FormResponse("city",city,"City not found");
        }

        [HttpPost]
        public ActionResult<ResponseModel<CityModel>> Create(CityModel cityModel)
        {
             var city = _cityService.Create(cityModel);

             return ResponseModel<CityModel>.FormResponse("city",city,"Unable to create city");
        }

        [HttpPut("{id}")]
        public ActionResult<ResponseModel<CityModel>> Update(string id,CityModel cityModel)
        {
            var city = _cityService.Get(id);

            return ResponseModel<CityModel>.FormResponse("city",city,"Unable to find city for updating");
        }
        
        [HttpDelete("{id}")]
        public ActionResult<ResponseModel<string>> Delete(string id)
        {
            var city = _cityService.Get(id);
            if (city != null)
            {
                _cityService.Delete(id);
                return new ResponseModel<string>(null);
            }

            return new ResponseModel<string>(null, false, new[] {"Unable to find city for deleting"});
        }
    }
}