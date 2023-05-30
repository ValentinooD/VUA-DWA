using System.ComponentModel.DataAnnotations;

namespace IntegrationModule.Models.Responses
{
    public class NotificationResponse
    {
        public int Id { get; set; }

        [Required]
        public string ReceiverEmail { get; set; }

        [Required]
        public string Subject { get; set; }

        [Required]
        public string Body { get; set; }
        
        public DateTime? SentAt { get; set; }
    }
}
