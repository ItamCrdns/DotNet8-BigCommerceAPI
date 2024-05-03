using BeachCommerce.Models;

namespace BeachCommerce.Abstractions
{
    public interface IUserRepository
    {
        string Login(User user);
    }
}
