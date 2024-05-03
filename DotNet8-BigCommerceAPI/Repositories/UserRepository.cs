using BeachCommerce.Abstractions;
using BeachCommerce.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BeachCommerce.Repositories
{
    public class UserRepository(IConfiguration configuration, IUserStore userStore) : IUserRepository
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IUserStore _userStore = userStore;

        public string Login(User user)
        {
            if (user is null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            {
                return null;
            }

            var loginUser = _userStore.GetUsers().SingleOrDefault(u => u.Username == user.Username && u.Password == user.Password);

            if (loginUser == null)
            {
                return null;
            }
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(3),
                Issuer = _configuration.GetValue<string>("JwtSettings:Issuer"),
                Audience = _configuration.GetValue<string>("JwtSettings:Audience"),
                SigningCredentials = new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("JwtSettings:Key"))), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
