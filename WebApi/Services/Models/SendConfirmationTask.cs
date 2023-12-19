namespace WebApi.Services.Models;

public class SendConfirmationTask : BackgroundTask
{
    public string Email { get; set; }
    public string Token { get; set; }
}