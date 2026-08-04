using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace CV_mng_sys.Core.Services;

public record SupportTicketRequest(
    string ReportedBy,
    string? Inventory,
    string Link,
    string Priority, //high, ave, low
    string Summary
);
public class SupportTicketService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    public SupportTicketService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }
    public async Task<(bool Success, string? Error)> SubmitTicketAsync(SupportTicketRequest ticket)
    {
        var adminEmails = _config.GetSection("SupportTickets:AdminEmails").Get<string[]>() ?? Array.Empty<string>();
        var payload = new
        {
            reportedBy = ticket.ReportedBy,
            inventory = ticket.Inventory,
            link = ticket.Link,
            priority = ticket.Priority,
            summary = ticket.Summary,
            adminEmails,
            submittedAtUtc = DateTime.UtcNow.ToString("o")
        };
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        var fileName = $"ticket-{DateTime.UtcNow:yyyyMMddHHmmssfff}.json";
    
        try{
            var accessToken = _config["Dropbox:AccessToken"];
            if (string.IsNullOrWhiteSpace(accessToken)) return (false, "Dropbox access token not found.");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var dropboxApiArg = JsonSerializer.Serialize(new
            {
                path = $"/support-tickets/{fileName}",
                mode = "add",
                autorename = true,
                mute = false
            });
            _httpClient.DefaultRequestHeaders.Remove("Dropbox-API-Arg");
            _httpClient.DefaultRequestHeaders.Add("Dropbox-API-Arg", dropboxApiArg);
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var response = await _httpClient.PostAsync("https://content.dropboxapi.com/2/files/upload", content);
            if (!response.IsSuccessStatusCode)
            {
                var err=  await response.Content.ReadAsStringAsync();
                return (false, $"Dropbox upload failed: {err}");
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Support ticket submission error: {ex.Message}");
        }
    }
}
