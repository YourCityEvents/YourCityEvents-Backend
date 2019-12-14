using System.ComponentModel.DataAnnotations;
using YourCityEventsApi.Model;

namespace YourCityEventsApi.Security
{
    public class UserRegistrationRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        
        public string Password { get; set; }
        
        public CityModel City { get; set; }
    }
}