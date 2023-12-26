namespace WebApi.Services.Models;

public class SaveTemporaryFileResult
{
    public bool Success { get; set; }
    public string? Reason { get; set; }
    public Guid? WorkSpaceId { get; set; }
}