﻿namespace WebApplication1.EmailSender;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string message);
}