namespace Domain.Model.View;

public class UploadVideoModel
{
    public MemoryStream FileStream { get; set; } 
    public string FileName { get; set; }
    public long FileSize { get; set; }
}