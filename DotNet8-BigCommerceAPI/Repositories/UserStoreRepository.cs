using BeachCommerce.Abstractions;
using BeachCommerce.Models;

namespace BeachCommerce.Repositories
{
    public class UserStoreRepository : IUserStore
    {
        public List<User> GetUsers()
        {
            return
            [
                new User { Username = "username", Password = "password" }
            ];
        }
    }
}
