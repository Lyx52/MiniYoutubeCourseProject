namespace WebApi.Services.Models;

public class ProcessingVideoTaskResult
{
    public Guid VideoId { get; set; }
    public bool Successful { get; set; }
}