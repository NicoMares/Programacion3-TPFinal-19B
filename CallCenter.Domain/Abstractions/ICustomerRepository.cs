using CallCenter.Domain.Entities;

namespace CallCenter.Domain.Abstractions
{
    public interface ICustomerRepository
    {
        bool ExistsByDocumentOrEmail(string document, string email);
        int Insert(Customer c);
    }
}
