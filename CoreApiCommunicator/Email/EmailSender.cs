namespace CoreApiCommunicator.Email;

using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Threading.Tasks;

public class EmailSender : IEmailSender
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUser;
    private readonly string _smtpPass;

    public EmailSender(IConfiguration configuration)
    {
        _smtpServer = configuration["Smtp:Server"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(configuration["Smtp:Port"] ?? "587");
        _smtpUser = configuration["Smtp:User"] ?? "";
        _smtpPass = configuration["Smtp:Password"] ?? "";
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_smtpUser));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = body };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_smtpServer, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_smtpUser, _smtpPass);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}

