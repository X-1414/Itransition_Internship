using System.Text;
using System.Text.Json;

namespace AdminDashboard.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string toName, string verificationLink);
}
/// Sends email via the Brevo HTTP API (https://api.brevo.com/v3/smtp/email) instead of raw SMTP. 
public class EmailService : IEmailService
{
    private static readonly HttpClient httpClient = new HttpClient
    {
        BaseAddress = new Uri("https://api.brevo.com/")
    };
    
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
            var apiKey = section["BrevoApiKey"];
            var fromAddress = section["FromAddress"];
            var fromName = section["FromName"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogWarning("Email: BrevoApiKey is not configured. Skipping email to {Email}.", toEmail);
                return;
            }

            var html = $@"
                <div style=""font-family:Arial, sans-serif; max-width:520px; margin:0 auto; "">
                    <h2 style="" color:#0d6efd;"">THE APP</h2>
                    <p>Hello {System.Net.WebUtility.HtmlEncode(toName)},</p>
                    <p>Please verify your email address by clicking the button below.</p>
                    <p><a href=""{verificationLink}"" style=""display:inline-block;padding:10px 24px; background:#0d6efd;color:#fff;
                                text-decoration:none;border-radius:4px;font-size:14px;""> Verify Email</a></p>
                    <p style=""color:#6c757d;font-size:12px;"">If you did not register, ignore this message.</p>
                </div>";

            var payload = new
            {
                sender = new { email = fromAddress, name = fromName },
                to = new[] { new { email = toEmail, name = toName }},
                subject = "Verify your email - The App",
                htmlContent = html
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "v3/smtp/email")
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("api-key", apiKey);
            request.Headers.Add("Accept", "application/json");

            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Brevo API returned {StatusCode} sending to {Email}: {Body}", response.StatusCode, toEmail, body);
            }
        } catch (Exception ex)
        {
            _logger.LogWarning("Failed to send verification email to {Email}: {Message}", toEmail, ex.Message);
        }
    }
}