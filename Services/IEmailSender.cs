using System.Threading.Tasks;

namespace Progra3_TPFinal_19B.Services
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }
}
