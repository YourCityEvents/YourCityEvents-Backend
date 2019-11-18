using System.ComponentModel.DataAnnotations;

namespace YourCityEventsApi.Model
{
    public class ChangeEmailRequest
    {
        public string Password { get; set; }
        
        [EmailAddress]
        public string NewEmail { get; set; }
    }
}