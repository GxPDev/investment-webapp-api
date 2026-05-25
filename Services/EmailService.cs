using System;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace InvestmentWebApp.Services
{
    public class EmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly bool _enableSsl;
        private readonly string? _smtpUser;
        private readonly string? _smtpPass;

        public EmailService(IConfiguration configuration)
        {
            var emailSettings = configuration.GetSection("EmailSettings");
            _smtpHost = emailSettings.GetValue<string?>("SmtpHost") ?? "smtp.gmail.com";
            _smtpPort = emailSettings.GetValue<int?>("SmtpPort") ?? 587;
            _enableSsl = emailSettings.GetValue<bool?>("EnableSsl") ?? true;
            _smtpUser = emailSettings.GetValue<string?>("SmtpUser");
            _smtpPass = emailSettings.GetValue<string?>("SmtpPass");
        }

        public void SendEmail(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(to))
                return;

            if (string.IsNullOrWhiteSpace(_smtpUser) || string.IsNullOrWhiteSpace(_smtpPass))
                return;

            using var message = new MailMessage();
            message.From = new MailAddress(_smtpUser, "Investment Web App");
            message.To.Add(new MailAddress(to));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = false;

            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                EnableSsl = _enableSsl
            };

            client.Send(message);
        }
    }
}
