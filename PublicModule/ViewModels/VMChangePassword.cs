using System.ComponentModel;

namespace PublicModule.ViewModels
{
    public class VMChangePassword
    {
        [DisplayName("User name")]
        public string Username { get; set; }
        [DisplayName("Password")]
        public string Password { get; set; }
        [DisplayName("New Password")]
        public string NewPassword { get; set; }
    }
}
