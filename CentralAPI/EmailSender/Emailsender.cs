using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace WebApplication1.EmailSender
{
    public class EmailSender : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        public EmailSender(EmailSettings emailSettings)
        {
            _emailSettings = emailSettings;
        }
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                emailMessage.To.Add(MailboxAddress.Parse(email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("html") { Text = message };

                using var client = new SmtpClient();
        
                client.Connected += (s, e) => Console.WriteLine("Connected to SMTP");
                client.Authenticated += (s, e) => Console.WriteLine("Successful authentication");
        
                await client.ConnectAsync(
                    _emailSettings.SmtpServer, 
                    _emailSettings.SmtpPort, 
                    MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.SenderPassword);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
        
                Console.WriteLine("List sended");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
                throw;
            }
        }
    }
}