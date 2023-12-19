using System.Net.Mail;

namespace WebApi.Services.Interfaces;

public interface IEmailProcessingService
{
    Task SendEmail(string to, string subject, string body, CancellationToken cancellationToken = default(CancellationToken));
    Task SendConfirmationEmail(string to, string token, CancellationToken cancellationToken = default(CancellationToken));
}