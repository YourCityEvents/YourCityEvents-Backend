using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Rewrite.Internal.PatternSegments;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.ExpressionTranslators.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace YourCityEventsApi.Model
{
    public class UserModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [JsonIgnore]
        [BsonElement("password")]
        public string Password { get; set; }
        
        [BsonElement("first_name")]
        public string FirstName { get; set; }
        
        [BsonElement("last_name")]
        public string LastName { get; set; }
        
        [BsonElement("bio")]
        public string Bio { get; set; }
        
        [EmailAddress]
        [BsonElement("email")]
        public string Email { get; set; }
        
        [BsonElement("city")]
        public CityModel City { get; set; }

        [BsonElement("hosting_events")]
        public EventModel[] HostingEvents { get; set; }
        
        [BsonElement("visiting_events")]
        public EventModel[] VisitingEvents { get; set; }

        [BsonElement("image_url")]
        public string ImageUrl { get; set; }
        
        [JsonIgnore]
        [BsonElement("token")]
        public string Token { get; set; }

        public UserModel(string id, string email,string password,string firstName
            , string lastName, CityModel city,EventModel[] hostingEvents=null
            , EventModel[] visitingEvents=null, string imageUrl=null,string bio=null
            ,string token=null)
        {
            Id = id;
            Password = password;
            FirstName = firstName;
            LastName = lastName;
            Bio = bio;
            Email = email.ToLower();
            City = city;
            HostingEvents = hostingEvents;
            VisitingEvents = visitingEvents;
            ImageUrl = imageUrl;
            Token = token;
        }
        
        
    }
}