namespace Domain.Model.Response;

public class UploadVideoFileResponse : Response
{
    public Guid FileId { get; set; }
    public string FileName { get; set; }
}