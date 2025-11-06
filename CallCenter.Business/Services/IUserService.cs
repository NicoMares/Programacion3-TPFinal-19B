namespace CallCenter.Business.Services
{
    public interface IUserService
    {
        bool ValidateCredentials(string email, string plainPassword, out string role);
    }
}
