namespace WebApplication1.EmailSender;

public class UseEmailSender
{
    private readonly IEmailService _emailService;

    public UseEmailSender(IEmailService emailService)
    {
        _emailService = emailService;
    }

  
    public async Task SendPasswordResetLink(string email, string resetCode)
    {
        var subject = "Скидання паролю ";
        var body = $"Ваш код для скидання паролю: <strong>{resetCode}</strong>. " +
                   "Будь ласка, введіть цей код, щоб скинути ваш поточний пароль.";

        await _emailService.SendEmailAsync(email, subject, body);
    }
}