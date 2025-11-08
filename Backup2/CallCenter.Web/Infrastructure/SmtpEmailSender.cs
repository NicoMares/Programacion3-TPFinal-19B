using System.Net;
using System.Net.Mail;
using CallCenter.Business.Services;

namespace CallCenter.Web.Infrastructure
{
    public class SmtpEmailSender : IEmailSender
    {
        public bool Send(string to, string subject, string htmlBody)
        {
            // Configuración Gmail
            string host = "smtp.gmail.com";
            int port = 587;
            string user = System.Configuration.ConfigurationManager.AppSettings["MailUser"];
            string pass = System.Configuration.ConfigurationManager.AppSettings["MailPass"];
            try
            {
                SmtpClient cli = new SmtpClient(host, port);
                cli.EnableSsl = true;
                cli.Credentials = new NetworkCredential(user, pass);

                MailMessage msg = new MailMessage();
                msg.From = new MailAddress(user, "CallCenter App");
                msg.To.Add(to);
                msg.Subject = subject;
                msg.Body = htmlBody;
                msg.IsBodyHtml = true;

                cli.Send(msg);
                return true;
            }
            catch (SmtpException ex)
            {
                System.Diagnostics.Debug.WriteLine("SMTP ERROR: " + ex.Message);
                return false;
            }
        }
    }
}
