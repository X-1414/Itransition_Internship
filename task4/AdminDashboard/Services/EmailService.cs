using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace AdminDashboard.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string toName, string verificationLink);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger <EmailService> _logger;

    public EmailService(IConfiguration config, ILogger <EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendVerificationEmailAsync(string toEmail, string toName, string verificationLink)
    {
        try
        {
            var section = _config.GetSection("Email");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(section["FromName"], section["FromAddress"]!));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = "Verify your email - The Admin Dashboard App";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <div style=""font-family:Arial, sans-serif; max-width:520px; margin:0 auto; "">
                        <h2 style="" color:#0d6efd;"">THE APP</h2>
                        <p>Hello {System.Web.HttpUtility.HtmlEncode(toName)},</p>
                        <p>Please verify your email address by clicking the button below.</p>
                        <p><a href=""{verificationLink}"" style=""display:inline-block;padding:10px 24px; background:#0d6efd;color:#fff;
                                    text-decoration:none;border-radius:4px;font-size:14px;""> Verify Email</a></p>
                        <p style="" color:#6c757d;font-size:12px;"">If you did not register, ignore this message.</p>
                    </div>"
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(section["SmtpHost"], int.Parse(section["SmtpPort"]!), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(section["SmtpUser"], section["SmtpPass"]); 
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        } catch (Exception ex)
        {
            _logger.LogWarning("Failed to send verification email to {Email}: {Message}", toEmail, ex.Message);
        }
    }
}