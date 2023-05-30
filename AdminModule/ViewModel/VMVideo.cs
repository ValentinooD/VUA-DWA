using DAL.Models;

namespace AdminModule.ViewModel
{
    public class VMVideo
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Genre { get; set; }
        public string? StreamingUrl { get; set; }
        public IFormFile Image { get; set; }
    }
}
