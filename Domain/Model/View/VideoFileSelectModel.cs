using System.ComponentModel.DataAnnotations;
using Domain.Attributes;
using Microsoft.AspNetCore.Components.Forms;

namespace Domain.Model.View;

public class VideoFileSelectModel
{
    // Max size 1GB
    [VideoFileValidation(1024 * 1024 * 1024,"video/mp4", "video/webm", "video/avi")]
    [Required]
    public IBrowserFile? File { get; set; } 
}