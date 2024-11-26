using CryptoOrganizerWebAPI.Interfaces;
using MailKit.Security;
using MimeKit;

namespace CryptoOrganizerWebAPI.Services
{
    public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(configuration["Email:AppName"], configuration["Email:From"]));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            var builder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            message.Body = builder.ToMessageBody();

            using var smtpClient = new MailKit.Net.Smtp.SmtpClient();
            try
            {
                await smtpClient.ConnectAsync(configuration["Email:SmtpHost"], int.Parse(configuration["Email:SmtpPort"]!), SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(configuration["Email:From"], configuration["Email:Password"]);
                await smtpClient.SendAsync(message);
                await smtpClient.DisconnectAsync(true, default);

                logger.LogInformation("Email sent successfully to: {EmailAddress}", email);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending email to: {EmailAddress}", email);
            }
        }
    }
}