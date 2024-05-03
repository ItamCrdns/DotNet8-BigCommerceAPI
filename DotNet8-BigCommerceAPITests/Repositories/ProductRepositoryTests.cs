using BeachCommerce.Abstractions;
using BeachCommerce.Common;
using BeachCommerce.Dto;
using BeachCommerce.Models;
using BeachCommerce.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;

namespace BeachCommerceTests.Repositories
{
    public class ProductRepositoryTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new();
        private readonly Mock<HttpMessageHandler> _handlerMock = new();
        private readonly Mock<IBrandRepository> _brandRepositoryMock = new();

        [Fact]
        public async Task CreateProduct_ReturnsProductCreated()
        {
            // Arrange
            var product = new NewProduct
            {
                Name = "Test Product 999",
                Type = "Test Type 999",
                BrandName = "Test brand 999",
                Sku = "test-product-999",
                Weight = 1,
                Price = 1,
                InventoryLevel = 25
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Product> { Data = new Product { Name = product.Name, Type = product.Type } }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.CreateProduct(product);

            // Assert
            result.Success.Should().BeTrue();
            result.Message = "Product created successfully";
            result.StatusCode = (int)HttpStatusCode.OK;
            result.Data.Should().NotBeNull();
            result.Data.Data.Name.Should().Be(product.Name);
            result.Data.Data.Type.Should().Be(product.Type);
        }

        [Fact]
        public async Task CreateProduct_ReturnsMissingFields()
        {
            // Arrange
            var product = new NewProduct
            {
                Name = "",
                Type = "",
                Weight = 0,
                Price = 0
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(JsonSerializer.Serialize(new Error { Errors = new { Name = "Product name is required.", Type = "Product type is required.", Weight = "Product weight is required.", Price = "Product price is required." } }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.CreateProduct(product);

            // Assert
            result.Success.Should().BeFalse();
            result.Message = "Product name, type, weight, and price are required fields.";
            result.StatusCode = (int)HttpStatusCode.BadRequest;
            result.Errors.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateProduct_ReturnsPartialSuccess()
        {
            // Arrange
            var product = new NewProduct
            {
                Name = "Test Product 999",
                Type = "Test Type 999",
                BrandName = "Test brand 999",
                Sku = "test-product-999",
                Weight = 1,
                Price = 1,
                InventoryLevel = 25
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = (HttpStatusCode)207,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Product> { Data = new Product() }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.CreateProduct(product);

            // Assert
            result.Success.Should().BeTrue();
            result.Message = "The request was partially successful. Some operations may have failed.";
            result.StatusCode = (int)HttpStatusCode.MultiStatus;
            result.Errors.Should().BeNull();
        }

        [Fact]
        public async Task CreateProduct_ReturnsConflict()
        {
            // Arrange
            var product = new NewProduct
            {
                Name = "Test Product 999",
                Type = "Test Type 999",
                Weight = 1,
                Price = 1
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Conflict,
                Content = new StringContent(JsonSerializer.Serialize(new Error { Title = "Conflict", Errors = new { Name = "Product name is required.", Type = "Product type is required.", Weight = "Product weight is required.", Price = "Product price is required." } }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.CreateProduct(product);

            // Assert
            result.Success.Should().BeFalse();
            result.Message = "Conflict";
            result.StatusCode = (int)HttpStatusCode.Conflict;
            result.Errors.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateProduct_ReturnsUnprocessableEntity()
        {
            // Arrange
            var product = new NewProduct
            {
                Name = "Test Product 999",
                Type = "Test Type 999",
                Weight = 1,
                Price = 1
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.UnprocessableEntity,
                Content = new StringContent(JsonSerializer.Serialize(new Error { Title = "Unprocessable Entity", Errors = new { AnyError = "Error" } }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.CreateProduct(product);

            // Assert
            result.Success.Should().BeFalse();
            result.Message = "Unprocessable Entity";
            result.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            result.Errors.Should().NotBeNull();
        }

        [Fact]
        public async Task GetAllProducts_ReturnsAllProducts()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Product[]> { Data = [new Product { Name = "Test Product 999", Type = "Test Type 999" }] }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.GetAllProducts(1, 50);

            // Assert
            result.Data.Should().HaveCountGreaterThanOrEqualTo(1);
            result.Data[0].Name.Should().Be("Test Product 999");
            result.Data[0].Type.Should().Be("Test Type 999");
        }

        [Fact]
        public async Task GetAProduct_ReturnsProduct()
        {
            // Arrange
            var productId = 1;

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Product> { Data = new Product { Name = "Test Product 999", Type = "Test Type 999" } }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.GetAProduct(productId);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Data.Name.Should().Be("Test Product 999");
            result.Data.Data.Type.Should().Be("Test Type 999");
        }

        [Fact]
        public async Task GetAProduct_ReturnsNotFound()
        {
            // Arrange
            var productId = 999;

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(JsonSerializer.Serialize(new OperationResult<MetaData<Product>> { Success = false, Message = "Product not found" }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.GetAProduct(productId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message = "Product not found";
            result.StatusCode = (int)HttpStatusCode.NotFound;
        }

        [Fact]
        public async Task GetProductImages_ReturnsImages()
        {
            // Arrange
            var productId = 1;

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Image[]> { Data = [new Image { UrlStandard = "https://example.com/image.jpg" }] }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.GetProductImages(productId, 1, 50);

            // Assert
            result.Data.Data.Should().HaveCountGreaterThanOrEqualTo(1);
            result.Data.Data[0].UrlStandard.Should().Be("https://example.com/image.jpg");
        }

        [Fact]
        public async Task GetProductImages_ReturnsProductExistsButNoImages()
        {
            // Arrange
            var productId = 999;

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent,
                Content = new StringContent(string.Empty),

            };

            // Mock the HttpClient response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.GetProductImages(productId, 1, 50);

            // Assert
            result.Success.Should().BeFalse();
            result.Message = "This product does not have any images";
            result.StatusCode = (int)HttpStatusCode.NoContent;
        }

        [Fact]
        public async Task GetProductImages_ReturnsProductDoesNotExist()
        {
            // Arrange
            var productId = 999;

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(JsonSerializer.Serialize(new OperationResult<MetaData<Image[]>> { Success = false, Message = "Product not found" }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.GetProductImages(productId, 1, 50);

            // Assert
            result.Success.Should().BeFalse();
            result.Message = "The product ID does not exist";
            result.StatusCode = (int)HttpStatusCode.NotFound;
        }

        [Fact]
        public async Task CreateProductImage_ReturnsImageCreated()
        {
            // Arrange
            var productId = 1;
            // Mock the image because we are calling the OpenReadStream
            var imageMock = new Mock<IFormFile>();

            imageMock.Setup(_ => _.FileName).Returns("image.jpg");
            imageMock.Setup(_ => _.ContentType).Returns("image/jpeg");
            imageMock.Setup(_ => _.Length).Returns(1014);
            imageMock.Setup(_ => _.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("Test Image")));

            var image = imageMock.Object;

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Image> { Data = new Image { UrlStandard = "https://example.com/image.jpg" } }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.CreateProductImage(productId, image);

            // Assert
            result.Success.Should().BeTrue();
            result.Message = "Image created successfully";
            result.StatusCode = (int)HttpStatusCode.OK;
            result.Data.Should().NotBeNull();
            result.Data.Data.UrlStandard.Should().Be("https://example.com/image.jpg");
        }

        [Fact]
        public async Task CreateProductImage_ReturnsProductDoesNotExist()
        {
            // Arrange
            var productId = 999;

            // Mock the image because we are calling the OpenReadStream
            var imageMock = new Mock<IFormFile>();

            imageMock.Setup(_ => _.FileName).Returns("image.jpg");
            imageMock.Setup(_ => _.ContentType).Returns("image/jpeg");
            imageMock.Setup(_ => _.Length).Returns(1014);
            imageMock.Setup(_ => _.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("Test Image")));

            var image = imageMock.Object;

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(JsonSerializer.Serialize(new OperationResult<MetaData<Image>> { Success = false, Message = "Product not found" }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.CreateProductImage(productId, image);

            // Assert
            result.Success.Should().BeFalse();
            result.Message = "Product not found";
            result.StatusCode = (int)HttpStatusCode.NotFound;
        }

        [Fact]
        public async Task CreateProductImage_ReturnsBadRequest()
        {
            // Arrange
            var productId = 1;

            // Mock the image because we are calling the OpenReadStream
            var imageMock = new Mock<IFormFile>();

            imageMock.Setup(_ => _.FileName).Returns("image.jpg");
            imageMock.Setup(_ => _.ContentType).Returns("image/jpeg");
            imageMock.Setup(_ => _.Length).Returns(1014);
            imageMock.Setup(_ => _.OpenReadStream()).Returns(new MemoryStream(Encoding.UTF8.GetBytes("Test Image")));

            var image = imageMock.Object;

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(JsonSerializer.Serialize(new Error { Title = "Bad Request", Errors = new { AnyError = "Error" } }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.CreateProductImage(productId, image);

            // Assert
            result.Success.Should().BeFalse();
            result.Message = "Bad Request";
            result.StatusCode = (int)HttpStatusCode.BadRequest;
            result.Errors.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteAProduct_ReturnsTrueDeleted()
        {
            // Arrange
            var productId = 1;

            var getAProductHttpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Product> { Data = new Product { Name = "Test Product 999", Type = "Test Type 999" } }), Encoding.UTF8, "application/json")
            };

            var deleteAProduct = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent,
                Content = new StringContent(string.Empty)
            };

            // Mock the HttpClient response
            // Call a setup sequencue because DeleteAProduct makes two HTTP requests
            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(getAProductHttpResponse) // First call to GetAProduct
                .ReturnsAsync(deleteAProduct); // Second call to DeleteAProduct

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.DeleteAProduct(productId);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Product deleted successfully");
            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task DeleteAProduct_ReturnsFalseProductDoesNotExist()
        {
            // Arrange
            var productId = 999;

            var getAProductHttpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(JsonSerializer.Serialize(new OperationResult<MetaData<Product>> { Success = false, Message = "Product not found" }), Encoding.UTF8, "application/json")
            };

            var deleteAProduct = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(JsonSerializer.Serialize(new OperationResult<bool> { Success = false, Message = "Product not found" }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            // Call a setup sequencue because DeleteAProduct makes two HTTP requests
            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(getAProductHttpResponse) // First call to GetAProduct
                .ReturnsAsync(deleteAProduct); // Second call to DeleteAProduct


            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.DeleteAProduct(productId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Product not found");
            result.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteProductImage_ReturnsTrueDeleted()
        {
            // Arrange
            var productId = 1;
            var imageId = 1;

            var getAProductHttpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Product> { Data = new Product { Name = "Test Product 999", Type = "Test Type 999" } }), Encoding.UTF8, "application/json")
            };

            var getProductImagesResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Image[]>
                {
                    Data = [new Image { UrlStandard = "https://example.com/image.jpg" }]
                }), Encoding.UTF8, "application/json")

            };

            var deleteProductImage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent,
                Content = new StringContent(string.Empty)
            };

            // Mock the HttpClient response
            // Call a setup sequencue because DeleteProductImage makes two HTTP requests
            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(getAProductHttpResponse) // First call to GetAProduct
                .ReturnsAsync(getProductImagesResponse) // Second call to GetProductImages
                .ReturnsAsync(deleteProductImage); // Third call to DeleteProductImage

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.DeleteProductImage(productId, imageId);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Image deleted successfully");
            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
        }

        [Fact]
        public async Task DeleteProductImage_ReturnsFalseProductDoesNotExist()
        {
            // Arrange
            var productId = 999;
            var imageId = 1;

            var getAProductHttpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(JsonSerializer.Serialize(new OperationResult<MetaData<Product>> { Success = false, Message = "Product not found" }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(getAProductHttpResponse); // First call to GetAProduct (and only because it stops here)

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.DeleteProductImage(productId, imageId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Product not found");
            result.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteProductImage_ReturnsFalseProductDoesNotHaveImages()
        {
            // Arrange
            var productId = 1;
            var imageId = 1;

            var getAProductHttpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Product> { Data = new Product { Name = "Test Product 999", Type = "Test Type 999" } }), Encoding.UTF8, "application/json")
            };

            var getProductImagesResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NoContent,
                Content = new StringContent(JsonSerializer.Serialize(new OperationResult<MetaData<Image[]>> { Success = false, Message = "This product does not have any images" }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            // Call a setup sequencue because DeleteProductImage makes two HTTP requests
            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(getAProductHttpResponse) // First call to GetAProduct
                .ReturnsAsync(getProductImagesResponse); // Second call to GetProductImages

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.DeleteProductImage(productId, imageId);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("This product does not have any images");
            result.StatusCode.Should().Be((int)HttpStatusCode.NoContent);
        }

        [Fact]
        public async Task UpdateAProduct_ReturnsProductUpdated()
        {
            // Arrange
            var productId = 1;
            var product = new ProductDto
            {
                Name = "Test Product 999",
                Type = "Test Type 999"
            };

            var getAProductHttpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Product> { Data = new Product { Name = "Test Product 999", Type = "Test Type 999" } }), Encoding.UTF8, "application/json")
            };

            var putProductHttpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Product> { Data = new Product { Name = product.Name, Type = product.Type } }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            // Call a setup sequencue because UpdateAProduct makes two HTTP requests
            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(getAProductHttpResponse) // First call to GetAProduct
                .ReturnsAsync(putProductHttpResponse); // Second call to UpdateAProduct

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.UpdateAProduct(productId, product);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Product updated successfully");
            result.StatusCode.Should().Be((int)HttpStatusCode.OK);
            result.Data.Data.Name.Should().Be(product.Name);
            result.Data.Data.Type.Should().Be(product.Type);
        }

        [Fact]
        public async Task UpdateAProduct_ReturnsProductDoesNotExist()
        {
            // Arrange
            var productId = 999;
            var product = new ProductDto
            {
                Name = "Test Product 999",
                Type = "Test Type 999"
            };

            var getAProductHttpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(JsonSerializer.Serialize(new OperationResult<MetaData<Product>> { Success = false, Message = "Product not found" }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            // Call a setup sequencue because UpdateAProduct makes two HTTP requests
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(getAProductHttpResponse); // First call to GetAProduct (and only because it stops here)

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.UpdateAProduct(productId, product);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Product not found");
            result.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateAProduct_ReturnsNoChanges()
        {
            // Arrange
            var productId = 1;
            var product = new ProductDto
            {
                Name = "",
                Type = "",
                InventoryLevel = -1
            };

            var getAProductHttpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Product> { Data = new Product { Name = "Test Product 999", Type = "Test Type 999" } }), Encoding.UTF8, "application/json")
            };

            var putProductHttpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent(JsonSerializer.Serialize(new Error { Title = "Bad Request", Errors = new { AnyError = "Error" } }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            // Call a setup sequencue because UpdateAProduct makes two HTTP requests
            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(getAProductHttpResponse) // First call to GetAProduct
                .ReturnsAsync(putProductHttpResponse); // Second call to UpdateAProduct

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.UpdateAProduct(productId, product);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No changes were provided to update the product");
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            result.Errors.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAProduct_ReturnsConflict()
        {
            // Arrange
            var productId = 1;
            var product = new ProductDto
            {
                Name = "Test Product 999",
                Type = "Test Type 999"
            };

            var getAProductHttpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new MetaData<Product> { Data = new Product { Name = "Test Product 999", Type = "Test Type 999" } }), Encoding.UTF8, "application/json")
            };

            var putProductHttpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Conflict,
                Content = new StringContent(JsonSerializer.Serialize(new Error { Title = "Conflict", Errors = new { AnyError = "Error" } }), Encoding.UTF8, "application/json")
            };

            // Mock the HttpClient response
            // Call a setup sequencue because UpdateAProduct makes two HTTP requests
            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(getAProductHttpResponse) // First call to GetAProduct
                .ReturnsAsync(putProductHttpResponse); // Second call to UpdateAProduct

            _httpClientFactoryMock.Setup(x => x.CreateClient("BigCommerce"))
                .Returns(new HttpClient(_handlerMock.Object)
                {
                    BaseAddress = new Uri("https://api.bigcommerce.com/stores/protected/v3")
                });

            // Act
            var productRepository = new ProductRepository(_httpClientFactoryMock.Object);

            var result = await productRepository.UpdateAProduct(productId, product);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Conflict");
            result.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
            result.Errors.Should().NotBeNull();
        }
    }
}
