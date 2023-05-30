using System.ComponentModel.DataAnnotations;

namespace IntegrationModule.Models.Requests
{
    public class VideoCreateRequest
    {
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public string GenreName { get; set; }
        [Required]
        public int TotalSeconds { get; set; }
        [Required]
        public string? StreamingUrl { get; set; }

    }
}
