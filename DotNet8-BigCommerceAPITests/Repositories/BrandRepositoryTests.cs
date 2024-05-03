using BeachCommerce.Dto;
using Moq;
using System.Text.Json;
using System.Net;
using BeachCommerce.Models;
using System.Text;
using Moq.Protected;
using BeachCommerce.Repositories;
using FluentAssertions;

namespace BeachCommerceTests.Repositories
{
    public class BrandRepositoryTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
        private readonly Mock<HttpMessageHandler> _handlerMock = new();

        [Fact]
        public async Task CreateBrand_ReturnsBrandCreated()
        {
            // Arrange
            var brand = new BrandDto { Name = "Test Brand" };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Brand> { Data = new Brand { Name = "New Brand" } }), Encoding.UTF8, "application/json")
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var brandRepository = new BrandRepository(_httpClientFactoryMock.Object);

            var result = await brandRepository.CreateBrand(brand);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Message.Should().Be("Brand created successfully");
        }

        [Fact]
        public async Task CreateBrand_ReturnsMissingFields()
        {
            // Arrange
            var brand = new BrandDto { Name = "" };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.UnprocessableEntity,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Brand> { Data = new Brand { Name = "New Brand" } }), Encoding.UTF8, "application/json")
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var brandRepository = new BrandRepository(_httpClientFactoryMock.Object);

            var result = await brandRepository.CreateBrand(brand);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Be("Brand name is required");
        }

        [Fact]
        public async Task CreateBrand_ReturnsPartialSuccess()
        {
            // Arrange
            var brand = new BrandDto { Name = "Test Brand" };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = (HttpStatusCode)207,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Brand> { Data = new Brand { Name = "New Brand" } }), Encoding.UTF8, "application/json")
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var brandRepository = new BrandRepository(_httpClientFactoryMock.Object);

            var result = await brandRepository.CreateBrand(brand);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Data.Should().NotBeNull();
            result.Message.Should().Be("The request was partially successful. Some operations may have failed.");
        }

        [Fact]
        public async Task CreateBrand_ReturnsConflict()
        {
            // Arrange
            var brand = new BrandDto { Name = "Test Brand" };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Conflict,
                Content = new StringContent(JsonSerializer.Serialize(new Error { Title = "Conflict" }), Encoding.UTF8, "application/json")
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var brandRepository = new BrandRepository(_httpClientFactoryMock.Object);

            var result = await brandRepository.CreateBrand(brand);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Data.Should().BeNull();
            result.Message.Should().Be("Conflict");
        }

        [Fact]
        public async Task GetAllBrands_ReturnsAllBrands()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Brand[]>
                {
                    Data =
                    [
                        new Brand { Name = "Brand 1" },
                        new Brand { Name = "Brand 2" }
                    ]
                }), Encoding.UTF8, "application/json")
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var brandRepository = new BrandRepository(_httpClientFactoryMock.Object);

            var result = await brandRepository.GetAllBrands();

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().NotBeNull();
            result.Data.Length.Should().Be(2);
            result.Data[0].Name.Should().Be("Brand 1");
            result.Data[1].Name.Should().Be("Brand 2");
        }

        [Fact]
        public async Task GetABrand_ReturnsBrand()
        {
            // Arrange
            var brandId = 1;

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Brand>
                {
                    Data = new Brand { Name = "Brand 1" }
                }), Encoding.UTF8, "application/json")
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var brandRepository = new BrandRepository(_httpClientFactoryMock.Object);

            var result = await brandRepository.GetABrand(brandId);

            // Assert
            result.Should().NotBeNull();
            result.Data.Name.Should().Be("Brand 1");
        }

        [Fact]
        public async Task GetABrand_ReturnsNotFound()
        {
            // Arrange
            var brandId = 1;

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(JsonSerializer.Serialize(new Error { Title = "Brand not found" }), Encoding.UTF8, "application/json")
            };

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var brandRepository = new BrandRepository(_httpClientFactoryMock.Object);

            var result = await brandRepository.GetABrand(brandId);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeNull();
            result.Message.Should().Be("Brand not found");
        }
    }
}
