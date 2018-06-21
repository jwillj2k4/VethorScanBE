using System.Threading.Tasks;

namespace VethorScan.Domain
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
