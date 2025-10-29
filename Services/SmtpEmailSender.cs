using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Progra3_TPFinal_19B.Models.Email;
using System.Threading.Tasks;

namespace Progra3_TPFinal_19B.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _cfg;
        public SmtpEmailSender(IOptions<EmailSettings> cfg) => _cfg = cfg.Value;

        public async Task SendAsync(string to, string subject, string htmlBody)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_cfg.FromName, _cfg.From));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subject;
            msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_cfg.Host, _cfg.Port,
                _cfg.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
            if (!string.IsNullOrWhiteSpace(_cfg.User))
                await smtp.AuthenticateAsync(_cfg.User, _cfg.Password);

            await smtp.SendAsync(msg);
            await smtp.DisconnectAsync(true);
        }
    }
}
