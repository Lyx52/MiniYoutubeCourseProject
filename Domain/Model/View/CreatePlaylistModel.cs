using System.ComponentModel.DataAnnotations;

namespace Domain.Model.Request;

public class CreatePlaylistModel
{
    [Required]
    [MinLength(8)]
    public string Title { get; set; }
    public List<Guid> Videos { get; set; } = new List<Guid>();
}