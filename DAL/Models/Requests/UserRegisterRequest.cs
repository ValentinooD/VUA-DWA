using System.ComponentModel.DataAnnotations;

namespace DAL.Models.Requests
{
    public class UserRegisterRequest
    {
        [Required, StringLength(50, MinimumLength = 6)]
        public string Username { get; set; }

        [Required, StringLength (50, MinimumLength = 1)]
        public string FirstName { get; set; }

        [Required, StringLength (50, MinimumLength = 1)]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string CountryCode { get; set; }

        public string Phone { get; set; }
    }
}
