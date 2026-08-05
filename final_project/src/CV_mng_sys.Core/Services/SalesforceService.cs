using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;

namespace CV_mng_sys.Core.Services;

public record SalesforceAccountContactRequest(
    string CompanyName,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    string? Title
);

public class SalesforceService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private string? _accessToken;
    private string? _instanceUrl;

    public SalesforceService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    private string BuildSignedJwt()
    {
        var consumerKey = _config["Salesforce:ConsumerKey"]!;
        var subjectUsername = _config["Salesforce:Username"]!;
        var audience = "https://login.salesforce.com";
        var pfxPath = Path.Combine(AppContext.BaseDirectory, "certs", "server.pfx");
        var pfxPassword = _config["Salesforce:PfxPassword"]!;
        var certificate = new X509Certificate2(pfxPath, pfxPassword);
        var signingCredentials = new X509SigningCredentials(certificate, SecurityAlgorithms.RsaSha256);
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = new JwtSecurityToken(
            issuer: consumerKey,
            audience: audience,
            claims: new[] { new System.Security.Claims.Claim("sub", subjectUsername) },
            expires: DateTime.UtcNow.AddMinutes(3),
            signingCredentials: signingCredentials
        );
        return tokenHandler.WriteToken(token);
    }
    private async Task AuthenticationAsync()
    {
        var jwt = BuildSignedJwt();
        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
            ["assertion"] = jwt
        };
        var response = await _httpClient.PostAsync("https://login.salesforce.com/services/oauth2/token", new FormUrlEncodedContent(tokenRequest));
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Salesforce auth failed ({response.StatusCode}): {errorBody}");
        }
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        _accessToken = doc.RootElement.GetProperty("access_token").GetString();
        _instanceUrl = doc.RootElement.GetProperty("instance_url").GetString();
    }
    public async Task<(bool Success, string? Error, string? AccountId, string? ContactId)> CreateAccountWithContactAsync(SalesforceAccountContactRequest request)
    {
        try
        {
            await AuthenticationAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            var accountBody = JsonSerializer.Serialize(new { Name = request.CompanyName });
            var accountResponse = await _httpClient.PostAsync($"{_instanceUrl}/services/data/v60.0/sobjects/Account/", new StringContent(accountBody, Encoding.UTF8, "application/json"));
            if (!accountResponse.IsSuccessStatusCode)
            {
                var err = await accountResponse.Content.ReadAsStringAsync();
                return (false, $"Account creation failed: {err}", null, null);
            }
            var accountJson = await accountResponse.Content.ReadAsStringAsync();
            var accountId = JsonDocument.Parse(accountJson).RootElement.GetProperty("id").GetString();
            var contactBody = JsonSerializer.Serialize(new
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                Title = request.Title,
                accountId = accountId
            });
            var contactResponse = await _httpClient.PostAsync($"{_instanceUrl}/services/data/v60.0/sobjects/Contact/", new StringContent(contactBody, Encoding.UTF8, "application/json"));
            if (!contactResponse.IsSuccessStatusCode)
            {
                var err = await contactResponse.Content.ReadAsStringAsync();
                return (false, $"Contact creation failed: {err}", accountId, null);
            }
            var contactJson = await contactResponse.Content.ReadAsStringAsync();
            var contactId = JsonDocument.Parse(contactJson).RootElement.GetProperty("id").GetString();
            return (true, null, accountId, contactId);
        }
        catch (Exception ex)
        {
            return (false, $"Salesforce integration error: {ex.Message}", null, null);
        }
    }
}