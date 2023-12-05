namespace Domain.Model.Request;

public class CreateVideoRequest
{
    public string Title { get; set; }
    public string Description { get; set; }
    public Guid WorkSpaceId { get; set; }
}