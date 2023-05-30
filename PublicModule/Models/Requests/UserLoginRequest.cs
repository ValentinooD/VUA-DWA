using System.ComponentModel.DataAnnotations;

namespace PublicModule.Models.Requests
{
    public class UserLoginRequest
    {
        [Required, StringLength(50, MinimumLength = 6)]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
