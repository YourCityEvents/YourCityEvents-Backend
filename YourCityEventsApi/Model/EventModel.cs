using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace YourCityEventsApi.Model
{
    public class EventModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("title")]
        public string Title { get; set; }
        
        [BsonElement("location")]
        public CityModel Location { get; set; }
        
        [BsonElement("description")]
        public string Description { get; set; }
        
        [BsonElement("owner")]
        public UserModel Owner { get; set; }
        
        [BsonElement("date")]
        public string Date { get; set; }
        
        [BsonElement("image_url")]
        public string ImageUrl { get; set; }
        
        [BsonElement("detail_location")]
        public string DetailLocation { get; set; }
        
        [BsonElement("visitors")]
        public UserModel[] Visitors { get; set; }
        
        [BsonElement("price")]
        public long Price { get; set; }

        public EventModel(string id, string title, CityModel location, string detailLocation,string description
            , UserModel owner,string date, long price,string imageUrl = null, UserModel[] visitors=null)
        {
            Id = id;
            Title = title;
            Location = location;
            DetailLocation = detailLocation;
            Description = description;
            Owner = owner;
            Date = date;
            ImageUrl = imageUrl;
            Visitors = visitors;
            Price = price;
        }

        public static EventModel ConvertToEventModel(BackendEventModel backendEventModel)
        {
            
            return new EventModel(backendEventModel.Id,backendEventModel.Title,backendEventModel.Location
            ,backendEventModel.DetailLocation,backendEventModel.Description,
            null,backendEventModel.Date,backendEventModel.Price,
            backendEventModel.ImageUrl);
        }
    }
}