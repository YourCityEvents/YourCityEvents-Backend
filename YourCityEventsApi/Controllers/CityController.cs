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
            var data=new Dictionary<string,List<CityModel>>();
            data.Add("cities",cityList);
            if (cityList != null)
            {
                return new ResponseModel<List<CityModel>>(data);
            }

            return new ResponseModel<List<CityModel>>(null, false, new[] {"Unable to get models"});
        }

        [HttpGet("{id}")]
        public ActionResult<ResponseModel<CityModel>> Get(string id)
        {
            var city = _cityService.Get(id);
            var data = new Dictionary<string, CityModel>();
            data.Add("city",city);
            if (city != null)
            {
                return new ResponseModel<CityModel>(data);
            }
            
            return new ResponseModel<CityModel>(null,false,new[]{"City not found"});
        }

        [HttpPost]
        public ActionResult<ResponseModel<CityModel>> Create(CityModel cityModel)
        {
             var city = _cityService.Create(cityModel);
             var data=new Dictionary<string,CityModel>();
             data.Add("city",city);
             if (city != null)
             {
                 return new ResponseModel<CityModel> (data);
             }
             
             return new ResponseModel<CityModel>(null,false,new[]{"Unable to create city"});
        }

        [HttpPut("{id}")]
        public ActionResult<ResponseModel<CityModel>> Update(string id,CityModel cityModel)
        {
            var city = _cityService.Get(id);
            var data=new Dictionary<string,CityModel>();
            data.Add("city",city);
            if (city != null)
            {
                _cityService.Update(id, cityModel);
                return new ResponseModel<CityModel>(data);
            }

            return new ResponseModel<CityModel>(null, false, new[] {"Unable to find city for updating"});
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