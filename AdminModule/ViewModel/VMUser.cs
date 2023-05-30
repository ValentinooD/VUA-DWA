using Microsoft.Build.Framework;

namespace AdminModule.ViewModel
{
    public class VMUser
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string? Phone { get; set; }

        [Required]
        public string CountryCode { get; set; }
    }
}
