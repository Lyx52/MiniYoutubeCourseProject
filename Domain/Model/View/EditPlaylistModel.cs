using System.ComponentModel.DataAnnotations;

namespace Domain.Model.View;

public class EditPlaylistModel
{
    public string? PlaylistId { get; set; }
    [Required]
    [MinLength(8)]
    public string Title { get; set; }
    public List<Guid> Videos { get; set; } = new List<Guid>();
}