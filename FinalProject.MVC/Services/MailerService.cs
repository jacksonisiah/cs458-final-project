using System.Net.Http.Headers;
using System.Text;
using FinalProject.MVC.Models;
using Microsoft.AspNetCore.Identity;

namespace FinalProject.MVC.Services;

public class MailerService
    : IEmailSender<ApplicationUser>,
        Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpFactory;

    public MailerService(IConfiguration configuration, IHttpClientFactory httpFactory)
    {
        _configuration = configuration;
        _httpFactory = httpFactory;
    }

    public Task SendEmailAsync(
        ApplicationUser user,
        string email,
        string subject,
        string htmlMessage
    )
    {
        return SendEmailInternalAsync(email, subject, htmlMessage);
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage) =>
        SendEmailInternalAsync(email, subject, htmlMessage);

    public Task SendConfirmationLinkAsync(
        ApplicationUser user,
        string email,
        string confirmationLink
    )
    {
        string subject = "Confirm your email";
        string message =
            $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.";
        return SendEmailInternalAsync(email, subject, message);
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        string subject = "Reset your password";
        string message = $"Please reset your password by <a href='{resetLink}'>clicking here</a>.";
        return SendEmailInternalAsync(email, subject, message);
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string code)
    {
        string subject = "Your password reset code";
        string message = $"Your password reset code is: {code}";
        return SendEmailInternalAsync(email, subject, message);
    }

    private async Task SendEmailInternalAsync(string email, string subject, string message)
    {
        using var client = _httpFactory.CreateClient();
        client.BaseAddress = new Uri("https://api.mailgun.net/v3/");

        var apiKey = _configuration["Mailgun:ApiKey"];
        var domain = _configuration["Mailgun:Domain"];
        var sender = _configuration["Mailgun:Sender"];

        if (
            string.IsNullOrEmpty(apiKey)
            || string.IsNullOrEmpty(domain)
            || string.IsNullOrEmpty(sender)
        )
        {
            throw new Exception("mailgun not configured correctly");
        }

        var authString = $"api:{apiKey}";
        var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(authString));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            authValue
        );

        var content = new MultipartFormDataContent
        {
            { new StringContent(sender), "from" },
            { new StringContent(email), "to" },
            { new StringContent(subject), "subject" },
            { new StringContent(message), "html" },
        };

        var response = await client.PostAsync($"{domain}/messages", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to send email: {response.StatusCode} - {errorMessage}");
        }
    }
}
