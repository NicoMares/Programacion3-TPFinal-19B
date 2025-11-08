using CallCenter.Domain.Entities;

namespace CallCenter.Domain.Abstractions
{
    public interface ICustomerRepository
    {
        bool ExistsByDocumentPhoneOrEmail(string document, string email, string phone);
        int Insert(Customer c);
    }
}
