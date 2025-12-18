using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Service.Service
{
  public class EmailSender : IEmailSender
    {

        private readonly EmailSetting _settings;
        public EmailSender(IOptions<EmailSetting> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var fromAddress = new MailAddress(_settings.FromEmail, _settings.FromName);
            var toAddress = new MailAddress(email);

            var smtp = new SmtpClient
            {
                Host = _settings.SmtpHost,
                Port = _settings.SmtpPort,
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.FromEmail, _settings.Password)
            };

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            await smtp.SendMailAsync(message);
        }
    }
}
