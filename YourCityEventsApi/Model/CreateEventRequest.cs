
namespace YourCityEventsApi.Model
{
    public class CreateEventRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public byte[] ImageArray { get; set; }
        public int Price { get; set; }
        public string DetailLocation { get; set; }
        public string Date { get; set; }

        public CreateEventRequest(string title, string description, byte[] imageArray
            , int price,string detailLocation, string date)
        {
            Title = title;
            Description = description;
            ImageArray = imageArray;
            Price = price;
            DetailLocation = detailLocation;
            Date = date;
        }
    }
}
