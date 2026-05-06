using System.Web;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Online_Booking_System.Contracts;
using Online_Booking_System.Settings;

namespace Online_Booking_System.Services
{
  public class EmailService : IEmailService
  {
    private readonly SmtpSettings _smtpSettings;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;
    private readonly IWebHostEnvironment _environment;

    public EmailService(
      IOptions<SmtpSettings> smtpSettings,
      IOptions<EmailSettings> emailSettings,
      ILogger<EmailService> logger,
      IWebHostEnvironment environment)
    {
      _smtpSettings = smtpSettings.Value;
      _emailSettings = emailSettings.Value;
      _logger = logger;
      _environment = environment;
    }

    public async Task SendConfirmationEmailAsync(string email, string userId, string token, string name)
    {
      var template = await LoadTemplateAsync("ConfirmEmail");
      var year = DateTime.Now.Year;

      var body = template
        .Replace("{{Name}}", name)
        .Replace("{{ConfirmationToken}}", token)
        .Replace("{{ConfirmUrl}}", $"{_emailSettings.BaseUrl}/Account/ConfirmEmail?userId={userId}&token={HttpUtility.UrlEncode(token)}")
        .Replace("{{Year}}", year.ToString());

      await SendEmailAsync(email, "Confirm Your Email", body);
    }

    public async Task SendPasswordResetEmailAsync(string email, string token, string name)
    {
      var template = await LoadTemplateAsync("PasswordReset");
      var year = DateTime.Now.Year;

      var body = template
        .Replace("{{Name}}", name)
        .Replace("{{ResetToken}}", token)
        .Replace("{{ResetUrl}}", $"{_emailSettings.BaseUrl}/Account/ResetPassword?userId={HttpUtility.UrlEncode(email)}&token={HttpUtility.UrlEncode(token)}")
        .Replace("{{Year}}", year.ToString());

      await SendEmailAsync(email, "Reset Your Password", body);
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
      if (_environment.IsDevelopment())
      {
        await SendViaSmtpAsync(to, subject, body);
      }
      else
      {
        LogEmail(to, subject, body);
      }
    }

    private async Task SendViaSmtpAsync(string to, string subject, string body)
    {
      try
      {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
          Text = body
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, _smtpSettings.UseSsl);

        if (!string.IsNullOrEmpty(_smtpSettings.Username) && !string.IsNullOrEmpty(_smtpSettings.Password))
        {
          await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        _logger.LogInformation("Email sent successfully to {Email}", to);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Failed to send email to {Email}. Error: {Message}", to, ex.Message);
        throw;
      }
    }

    private void LogEmail(string to, string subject, string body)
    {
      _logger.LogInformation("=== EMAIL (Development Mode - Not Sent) ===");
      _logger.LogInformation("To: {To}", to);
      _logger.LogInformation("Subject: {Subject}", subject);
      _logger.LogInformation("Body: {Body}", body.Substring(0, Math.Min(body.Length, 200)));
      _logger.LogInformation("=========================================");
    }

    private async Task<string> LoadTemplateAsync(string templateName)
    {
      var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", "Email", $"{templateName}.cshtml");

      if (!File.Exists(templatePath))
      {
        _logger.LogWarning("Email template not found: {Path}. Using fallback.", templatePath);
        return GetFallbackTemplate(templateName);
      }

      return await File.ReadAllTextAsync(templatePath);
    }

    private static string GetFallbackTemplate(string templateName)
    {
      return templateName switch
      {
        "ConfirmEmail" => "<html><body><h1>Confirm Email</h1><p>Hi {{Name}},</p><p>Your confirmation code: {{ConfirmationToken}}</p></body></html>",
        "PasswordReset" => "<html><body><h1>Reset Password</h1><p>Hi {{Name}},</p><p>Your reset code: {{ResetToken}}</p></body></html>",
        _ => "<html><body><h1>Email</h1><p>{{Message}}</p></body></html>"
      };
    }
  }
}