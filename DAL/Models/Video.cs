using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Models;

public partial class Video
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int GenreId { get; set; }

    public int TotalSeconds { get; set; }

    public string? StreamingUrl { get; set; }

    public int? ImageId { get; set; }

    public virtual Genre Genre { get; set; } = null!;

    public virtual Image? Image { get; set; }

    public virtual ICollection<VideoTag> VideoTags { get; set; } = new List<VideoTag>();

    // Util
    public string GetPrettyTotalTime()
    {

        StringBuilder builder = new StringBuilder();

        int h = TotalSeconds / 3600;
        if (h > 0)
        {
            builder.Append(h + "h ");
        }

        int m = (TotalSeconds / 60) % 60;
        if (m > 0)
        {
            builder.Append(m + "m ");
        }
        
        int s = TotalSeconds % 60;
        builder.Append(s + "s");

        return builder.ToString();
    }
}
