using Domain.Constants;

namespace Domain.Model;

public class WorkSpace
{
    public WorkSpaceDirectory Directory { get; set; }
    public Guid Id { get; set; }
    public List<WorkFile> Files { get; set; }
}