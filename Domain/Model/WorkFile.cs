using Domain.Constants;

namespace Domain.Model;

public class WorkFile
{
    public WorkFileType Type { get; set; }
    public Guid Id { get; set; }
    public string FileName { get; set; }
}