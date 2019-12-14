using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace YourCityEventsApi.Model
{
    public class CityModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("name_ua")]
        public string NameUa { get; set; }
        
        [BsonElement("name_en")]
        public string NameEn { get; set; }

        public CityModel(string id, string nameUa, string nameEn)
        {
            Id = id;
            NameUa = nameUa;
            NameEn = nameEn;
        }
    }
}