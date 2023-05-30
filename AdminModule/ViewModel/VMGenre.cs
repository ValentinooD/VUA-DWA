using Microsoft.Build.Framework;

namespace AdminModule.ViewModel
{
    public class VMGenre
    {
        [Required]
        public string Name { get; set; }

        public string? Description { get; set; }
    }
}
