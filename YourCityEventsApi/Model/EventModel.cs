using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using YourCityEventsApi.Services;

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
        public DateTime Date { get; set; }
        
        [BsonElement("image_urls")]
        public string ImageUrl { get; set; }
        
        [BsonElement("links")]
        public string[] Links { get; set; }
        
        [BsonElement("visitors")]
        public UserModel[] Visitors { get; set; }
        
        [BsonElement("price")]
        public long Price { get; set; }

        public EventModel(string id, string title, CityModel location, string description, UserModel owner
        ,DateTime date, long price,string imageUrl = null,string[] links=null, UserModel[] visitors=null)
        {
            Id = id;
            Title = title;
            Location = location;
            Description = description;
            Owner = owner;
            Date = date;
            ImageUrl = imageUrl;
            Links = links;
            Visitors = visitors;
            Price = price;
        }
    }
}