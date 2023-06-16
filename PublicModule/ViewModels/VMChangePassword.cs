using System.ComponentModel;

namespace PublicModule.ViewModels
{
    public class VMChangePassword
    {
        [DisplayName("OldPassword")]
        public string OldPassword { get; set; }

        [DisplayName("New Password")]
        public string NewPassword { get; set; }
        
        [DisplayName("Confirm Password")]
        public string ConfirmPassword { get; set; }
    }
}
