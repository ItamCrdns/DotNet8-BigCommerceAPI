using BeachCommerce.Models;

namespace BeachCommerce.Abstractions
{
    public interface IUserStore
    {
        List<User> GetUsers();
    }
}
