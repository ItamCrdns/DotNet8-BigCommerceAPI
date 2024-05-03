using BeachCommerce.Abstractions;
using BeachCommerce.Models;
using BeachCommerce.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace BeachCommerceTests.Repositories
{
    public class UserRepositoryTests
    {
        private readonly Mock<IConfiguration> _configuration = new();
        private readonly Mock<IUserStore> _userStore = new();

        [Fact]
        public void Login_WhenUserIsNull_ReturnsNull()
        {
            // Arrange
            var userRepository = new UserRepository(_configuration.Object, _userStore.Object);

            // Act
            var result = userRepository.Login(null);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Login_WhenUsernameIsNullOrEmpty_ReturnsNull()
        {
            // Arrange
            var userRepository = new UserRepository(_configuration.Object, _userStore.Object);

            // Act
            var result = userRepository.Login(new User { Username = "", Password = "password" });

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Login_WhenPasswordIsNullOrEmpty_ReturnsNull()
        {
            // Arrange
            var userRepository = new UserRepository(_configuration.Object, _userStore.Object);

            // Act
            var result = userRepository.Login(new User { Username = "username", Password = "" });

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Login_WhenUserDoesNotExist_ReturnsNull()
        {
            // Arrange
            var userRepository = new UserRepository(_configuration.Object, _userStore.Object);

            var mockGetUsers = _userStore.Setup(x => x.GetUsers()).Returns([new User { Username = "admin", Password = "admin" }]);

            // Act
            var result = userRepository.Login(new User { Username = "username", Password = "password" });

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Login_WithCorrectCredentials_ReturnsToken()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string>
            {
                { "JwtSettings:Key", "ed37afb23914b93b1fd9c0bbdb944080" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var userRepository = new UserRepository(configuration, _userStore.Object);

            var mockGetUsers = _userStore.Setup(x => x.GetUsers()).Returns([new User { Username = "admin", Password = "admin" }]);

            // Act
            var result = userRepository.Login(new User { Username = "admin", Password = "admin" });

            // Assert
            result.Should().NotBeNullOrEmpty();
        }
    }
}
