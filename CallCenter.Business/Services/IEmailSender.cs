namespace CallCenter.Business.Services
{
    public interface IEmailSender
    {
        bool Send(string to, string subject, string htmlBody);
    }
}
