namespace Domain.Model.Configuration;

public class SmtpConfiguration
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string EmailAddress { get; set; }
    public string EmailSender { get; set; }
    public string EmailPassword { get; set; }
}