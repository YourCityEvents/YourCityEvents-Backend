using System;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
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
        public string Owner { get; set; }
        
        [BsonElement("date")]
        public DateTime Date { get; set; }
        
        [BsonElement("image_urls")]
        public string[] ImageUrls { get; set; }
        
        [BsonElement("links")]
        public string[] Links { get; set; }
        
        [BsonElement("visitors")]
        public string[] Visitors { get; set; }
        
        [BsonElement("price")]
        public long Price { get; set; }

        public EventModel(string id, string title, CityModel location, string description, string owner
        ,DateTime date, long price,string[] imageUrls = null,string[] links=null, string[] visitors=null)
        {
            Id = id;
            Title = title;
            Location = location;
            Description = description;
            Owner = owner;
            Date = date;
            ImageUrls = imageUrls;
            Links = links;
            Visitors = visitors;
            Price = price;
        }
    }
}