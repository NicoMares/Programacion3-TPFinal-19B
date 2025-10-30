// Application/Contracts/ICustomerRepository.cs
using Progra3_TPFinal_19B.Models;

namespace Progra3_TPFinal_19B.Application.Contracts
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Customer>> ListAsync(int page, int size);
        Task CreateAsync(Customer c);
        Task UpdateAsync(Customer c);
        Task DeleteAsync(Guid id, Guid updatedBy);

        Task<bool> ExistsByDocumentAsync(string document);
        Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null);
    }
}
