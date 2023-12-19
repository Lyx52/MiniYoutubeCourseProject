using System.ComponentModel;
using System.Net;
using System.Text;
using Domain.Constants;
using Domain.Model.Configuration;
using MailKit.Net.Smtp;
using MimeKit;
using WebApi.Services.Interfaces;
using static Domain.Constants.EmailTemplates;

namespace WebApi.Services;

public class EmailProcessingService : IEmailProcessingService
{
    private readonly ILogger<EmailProcessingService> _logger;
    private readonly ApiConfiguration _configuration;
    private readonly MailboxAddress _fromAddress;
    private readonly string _hostname;
    public EmailProcessingService(ILogger<EmailProcessingService> logger, ApiConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
        _hostname = _configuration.Endpoints.First(c => c.Name == EndpointNames.MiniTubeApi.ToString()).Hostname;
        _fromAddress = new MailboxAddress(_configuration.Email.EmailSender, _configuration.Email.EmailAddress);
    }
    public async Task SendEmail(string to, string subject, string body, CancellationToken cancellationToken = default(CancellationToken))
    {
        var mail = new MimeMessage()
        {
            Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = body
            },
            Subject = subject,
        };
        mail.From.Add(_fromAddress);
        mail.To.Add(new MailboxAddress(to, to));
        
        try
        {
            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_configuration.Email.Host, _configuration.Email.Port, true, cancellationToken);
            await smtp.AuthenticateAsync(_configuration.Email.EmailAddress, _configuration.Email.EmailPassword,
                cancellationToken);
            await smtp.SendAsync(mail, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to send email with exception {ExceptionMessage}", e.Message);
        }
    }

    public Task SendConfirmationEmail(string to, string token, CancellationToken cancellationToken = default(CancellationToken))
    {
        string body = string.Format(ConfirmationEmailTemplate, _hostname, WebUtility.UrlEncode(token), to);
        return SendEmail(to, "MiniTube Confirmation Email", body, cancellationToken);
    }
}