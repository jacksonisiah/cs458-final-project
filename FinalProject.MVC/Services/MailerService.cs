using System;
using System.Net.Http.Headers;
using System.Text;

namespace FinalProject.MVC.Services;

public class MailerService(IConfiguration configuration, IHttpClientFactory httpFactory)
{
    public async Task SendEmailAsync(string destination, string message, string subject)
    {
        using var client = httpFactory.CreateClient();
        client.BaseAddress = new Uri("https://api.mailgun.net/v3/");

        // set auth
        var apiKey = configuration["Mailgun:ApiKey"];
        var authString = $"api:{apiKey}";
        var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes(authString));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            authValue
        );

        // set content
        MultipartFormDataContent content = new()
        {
            { new StringContent("Postmaster <postmaster@postmaster.jacksonisaiah.com>"), "from" },
            { new StringContent($"You <{destination}>"), "to" },
            { new StringContent(subject), "subject" },
            { new StringContent(message), "text" },
        };

        // send request
        var response = await client.PostAsync("postmaster.jacksonisaiah.com/messages", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to send email: {errorMessage}");
        }
    }
}
