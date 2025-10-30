// Application/Contracts/IUserRepository.cs (reemplazar por esta versión)
using Progra3_TPFinal_19B.Models;

namespace Progra3_TPFinal_19B.Application.Contracts
{
    public interface IUserRepository
    {
        Task<User?> FindForLoginAsync(string normalizedLogin); // Email o Username, !IsDeleted
        Task<bool> ExistsLoginAsync(string normalizedLogin);
        Task CreateAsync(User u);
        Task UpdatePasswordAsync(Guid userId, string newHexHash);
    }
}
