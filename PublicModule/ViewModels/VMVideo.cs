using DAL.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PublicModule.ViewModels
{
    public class VMVideo
    {
        public int Id { get; set; }

        [DisplayName("Name")]
        public string Name { get; set; } = null!;

        [DisplayName("Description")]
        public string? Description { get; set; }

        [DisplayName("Created at")]
        public DateTime CreatedAt { get; set; }

        [DisplayName("Streaming URL")]
        public string? StreamingUrl { get; set; }

        [DisplayName("Total seconds")]
        public int TotalSeconds { get; set; }

        [DisplayName("Genre")]
        public Genre? Genre { get; set; }
        public int? GenreId { get; set; }

        [DisplayName("Image")]
        public Image? Image { get; set; }
        public int? ImageId { get; set; }
    }
}
