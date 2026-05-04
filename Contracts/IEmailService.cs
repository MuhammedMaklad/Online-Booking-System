namespace Online_Booking_System.Contracts
{
  public interface IEmailService
  {
    Task SendConfirmationEmailAsync(string email, string userId, string token, string name);
    Task SendPasswordResetEmailAsync(string email, string token, string name);
    Task SendEmailAsync(string to, string subject, string body);
  }
}