using System.ComponentModel.DataAnnotations;

namespace Domain.Model;

public class CreateVideoModel
{
    [Required]
    [MinLength(10)]
    public string Title { get; set; }
    [Required]
    [MinLength(15)]
    public string Description { get; set; }
    public bool IsUnlisted { get; set; }
    [Required]
    public Guid WorkSpaceId { get; set; }
}