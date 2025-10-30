// Application/Contracts/IPasswordResetTokenRepository.cs (nuevo)
using Progra3_TPFinal_19B.Models;

namespace Progra3_TPFinal_19B.Application.Contracts
{
    public interface IPasswordResetTokenRepository
    {
        Task CreateAsync(PasswordResetToken token);
        Task<bool> IsValidAsync(string token);
        Task<PasswordResetToken?> GetByTokenAsync(string token);
        Task MarkUsedAsync(Guid id);
    }
}
