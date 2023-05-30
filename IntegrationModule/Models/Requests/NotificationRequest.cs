using System.ComponentModel.DataAnnotations;

namespace IntegrationModule.Models.Requests
{
    public class NotificationRequest
    {
        [EmailAddress]
        public string ReceiverEmail { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}
