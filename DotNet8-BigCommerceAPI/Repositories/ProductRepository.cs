using BeachCommerce.Abstractions;
using BeachCommerce.Common;
using BeachCommerce.Dto;
using BeachCommerce.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BeachCommerce.Repositories
{
    public class ProductRepository(IHttpClientFactory httpClientFactory) : IProductRepository
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        public async Task<OperationResult<MetaData<Product>>> CreateProduct(NewProduct product)
        {
            var client = _httpClientFactory.CreateClient("BigCommerce");

            // Field validations
            // Make sure to pass brandName otherwise update will throw error
            if (string.IsNullOrWhiteSpace(product.Name) ||
                string.IsNullOrWhiteSpace(product.Type) ||
                string.IsNullOrWhiteSpace(product.BrandName) ||
                string.IsNullOrWhiteSpace(product.Sku) ||
                product.Weight == 0 ||
                product.Price == 0 ||
                product.InventoryLevel == 0)
            {
                return new OperationResult<MetaData<Product>>
                {
                    Success = false,
                    Message = "Product name, type, brand, SKU, weight, price and inventory are required fields.",
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Errors = new Error { Errors = new { Error = "Please fill all necessary fields" } }
                };
            }

            string fullUrl = client.BaseAddress + "/catalog/products";

            var res = await client.PostAsync(fullUrl, new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json"));

            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadFromJsonAsync<MetaData<Product>>();
                return new OperationResult<MetaData<Product>>
                {
                    Success = true,
                    Message = "Product created successfully",
                    StatusCode = (int)HttpStatusCode.OK,
                    Data = content
                };
            }
            else if (res.StatusCode == HttpStatusCode.MultiStatus)
            {
                //Multi - status.Typically indicates that a partial failure has occurred, such as when a POST or PUT request is successful, but saving one of the attributes has failed.
                //For example, the product information was updated successfully, but the inventory data failed to update or saving the URL failed
                var content = await res.Content.ReadFromJsonAsync<MetaData<Product>>();
                return new OperationResult<MetaData<Product>>
                {
                    Success = false,
                    Message = "The request was partially successful. Some operations may have failed.",
                    StatusCode = (int)HttpStatusCode.MultiStatus,
                    Data = content
                };
            }
            else if (res.StatusCode == HttpStatusCode.Conflict)
            {
                var content = await res.Content.ReadFromJsonAsync<Error>();
                return new OperationResult<MetaData<Product>>
                {
                    Success = false,
                    Message = content.Title,
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Errors = content
                };
            }
            else
            {
                // Unprocessable Entity (422) status code
                var content = await res.Content.ReadFromJsonAsync<Error>();
                return new OperationResult<MetaData<Product>>
                {
                    Success = false,
                    Message = content.Title,
                    StatusCode = (int)HttpStatusCode.UnprocessableEntity,
                    Errors = content
                };
            }
        }

        public async Task<OperationResult<MetaData<Image>>> CreateProductImage(int productId, IFormFile image)
        {
            var client = _httpClientFactory.CreateClient("BigCommerce");

            int maxAllowedSize = 8 * 1024 * 1024; // 8MB in bytes
            var allowedExtensions = new[] { ".bmp", ".gif", ".jpeg", ".jpg", ".png", ".wbmp", ".xbm", ".webp" };

            if (image.Length > maxAllowedSize)
            {
                return new OperationResult<MetaData<Image>>
                {
                    Success = false,
                    Message = "The image size is too large. The maximum allowed size is 8MB.",
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }

            if (!allowedExtensions.Contains(Path.GetExtension(image.FileName)))
            {
                return new OperationResult<MetaData<Image>>
                {
                    Success = false,
                    Message = "The image file type is not supported. Supported file types are bmp, gif, jpeg, jpg, png, wbmp, xbm, and webp.",
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }

            // Tell the server how to interpret the file we are sending
            MultipartFormDataContent mfdc = new()
            {
                {
                    new StreamContent(image.OpenReadStream())
                    {
                        Headers =
                        {
                            ContentLength = image.Length,
                            ContentType = new MediaTypeHeaderValue(image.ContentType)
                        }
                    },
                    "image_file",
                    image.FileName
                }
            };

            string fullUrl = client.BaseAddress + $"/catalog/products/{productId}/images";

            var res = await client.PostAsync(fullUrl, mfdc);

            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadFromJsonAsync<MetaData<Image>>();
                return new OperationResult<MetaData<Image>>
                {
                    Success = true,
                    Message = "Image uploaded successfully",
                    StatusCode = (int)HttpStatusCode.OK,
                    Data = content
                };
            } else if (res.StatusCode == HttpStatusCode.NotFound)
            {
                var content = await res.Content.ReadFromJsonAsync<Error>();
                return new OperationResult<MetaData<Image>>
                {
                    Success = false,
                    Message = content.Title,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Errors = content
                };
            } else if (res.StatusCode == HttpStatusCode.BadRequest)
            {
                return new OperationResult<MetaData<Image>>
                {
                    Success = false,
                    Message = "Something went wrong",
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Errors = new Error { Title = "Bad Request" }
                };
            } else
            {
                // 422
                var content = await res.Content.ReadFromJsonAsync<Error>();
                return new OperationResult<MetaData<Image>>
                {
                    Success = false,
                    Message = content.Title,
                    StatusCode = (int)HttpStatusCode.UnprocessableEntity,
                    Errors = content
                };
            }
        }

        public async Task<OperationResult<bool>> DeleteAProduct(int productId)
        {
            var client = _httpClientFactory.CreateClient("BigCommerce");

            string fullUrl = client.BaseAddress + $"/catalog/products/{productId}";

            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var product = await GetAProduct(productId);

            if (product.StatusCode == 404)
            {
                return new OperationResult<bool>
                {
                    Success = false,
                    Message = "Product not found",
                    StatusCode = 404,
                };
            }

            var res = await client.DeleteAsync(fullUrl);

            if (res.IsSuccessStatusCode)
            {
                return new OperationResult<bool>
                {
                    Success = true,
                    Message = "Product deleted successfully",
                    StatusCode = 200,
                };
            }
            else
            {
                return new OperationResult<bool>
                {
                    Success = false,
                    Message = "Something went wrong",
                    StatusCode = 400,
                };
            }
        }

        public async Task<OperationResult<bool>> DeleteProductImage(int productId, int imageId)
        {
            var client = _httpClientFactory.CreateClient("BigCommerce");

            string fullUrl = client.BaseAddress + $"/catalog/products/{productId}/images/{imageId}";

            client.DefaultRequestHeaders.Add("Accept", "application/json");

            var product = await GetAProduct(productId);

            if (product.StatusCode == 404)
            {
                return new OperationResult<bool>
                {
                    Success = false,
                    Message = "Product not found",
                    StatusCode = 404,
                };
            }

            var productImages = await GetProductImages(productId, 1, 50);

            if (productImages.StatusCode == 204)
            {
                return new OperationResult<bool>
                {
                    Success = false,
                    Message = "This product does not have any images",
                    StatusCode = 204,
                };
            }

            var res = await client.DeleteAsync(fullUrl);

            if (res.IsSuccessStatusCode)
            {
                return new OperationResult<bool>
                {
                    Success = true,
                    Message = "Image deleted successfully",
                    StatusCode = 200,
                };
            }
            else
            {
                return new OperationResult<bool>
                {
                    Success = false,
                    Message = "Something went wrong",
                    StatusCode = 400,
                };
            }
        }

        public async Task<MetaData<ProductDto[]>> GetAllProducts(int? page, int? limit)
        {
            var client = _httpClientFactory.CreateClient("BigCommerce");

            string fullUrl = client.BaseAddress + $"/catalog/products?page={page}&limit={limit}";

            var res = await client.GetAsync(fullUrl);

            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadFromJsonAsync<MetaData<Product[]>>();
                //string brandName = _brandRepository.GetABrand(); 
                return new MetaData<ProductDto[]>
                {
                    Data = content.Data.Select(p => new ProductDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Type = p.Type,
                        Weight = p.Weight,
                        Price = p.Price,
                        BrandId = p.BrandId, // Could try to pass the name here, but that will mean to make a request for each brand since API does not return the name for some reason
                        Sku = p.Sku,
                        InventoryLevel = p.InventoryLevel
                    }).ToArray(),
                    Meta = content.Meta
                };
            }
            else
            {
                // Docs say this endpoint doesnt return any non 200 status code
                var errorContent = await res.Content.ReadAsStringAsync();
                throw new Exception(errorContent);
            }
        }

        public async Task<OperationResult<MetaData<ProductDto>>> GetAProduct(int productId)
        {
            var client = _httpClientFactory.CreateClient("BigCommerce");

            string fullUrl = client.BaseAddress + $"/catalog/products/{productId}";

            var res = await client.GetAsync(fullUrl);

            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadFromJsonAsync<MetaData<Product>>();
                return new OperationResult<MetaData<ProductDto>>
                {
                    Success = true,
                    Data = new MetaData<ProductDto>
                    {
                        Data = new ProductDto
                        {
                            Id = content.Data.Id,
                            Name = content.Data.Name,
                            Type = content.Data.Type,
                            Weight = content.Data.Weight,
                            Price = content.Data.Price,
                            BrandId = content.Data.BrandId,
                            //BrandName = content.Data.BrandName,
                            Sku = content.Data.Sku,
                            InventoryLevel = content.Data.InventoryLevel
                        },
                        Meta = content.Meta
                    },
                    Message = "Product retrieved successfully",
                    StatusCode = (int)HttpStatusCode.OK
                };
            }
            else
            {
                // 404
                var content = await res.Content.ReadFromJsonAsync<Error>();
                return new OperationResult<MetaData<ProductDto>>
                {
                    Success = false,
                    Message = content.Title,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Errors = content
                };
            }
        }

        public async Task<OperationResult<MetaData<Image[]>>> GetProductImages(int productId, int? page, int? limit)
        {
            var client = _httpClientFactory.CreateClient("BigCommerce");

            string fullUrl = client.BaseAddress + $"/catalog/products/{productId}/images?page={page}&limit={limit}";

            var res = await client.GetAsync(fullUrl);

            if (res.StatusCode == HttpStatusCode.OK)
            {
                var content = await res.Content.ReadFromJsonAsync<MetaData<Image[]>>();
                // For some reason docs say it returns 204 if no images, but no images also return 200 so make an extra check
                if (content.Data.Length == 0)
                {
                    return new OperationResult<MetaData<Image[]>>
                    {
                        StatusCode = 204
                    };
                }

                return new OperationResult<MetaData<Image[]>>
                {
                    Success = true,
                    Message = "Images found",
                    StatusCode = (int)HttpStatusCode.OK,
                    Data = content
                };
            }
            else if (res.StatusCode == (HttpStatusCode)204)
            {
                return new OperationResult<MetaData<Image[]>>
                {
                    Success = false,
                    Message = "This product does not have any images",
                    StatusCode = (int)HttpStatusCode.NoContent,
                    Data = new MetaData<Image[]>()
                };
            }
            else
            {
                var content = await res.Content.ReadFromJsonAsync<Error>();
                return new OperationResult<MetaData<Image[]>>
                {
                    Success = false,
                    Message = "The product ID does not exist",
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Errors = content
                };
            }
        }

        public async Task<OperationResult<MetaData<Product>>> UpdateAProduct(int productId, ProductDto product)
        {
            var client = _httpClientFactory.CreateClient("BigCommerce");

            string fullUrl = client.BaseAddress + $"/catalog/products/{productId}";

            var existingProduct = await GetAProduct(productId);

            if (existingProduct.StatusCode == 404)
            {
                return new OperationResult<MetaData<Product>>
                {
                    Success = false,
                    Message = "Product not found",
                    StatusCode = 404,
                };
            }

            var newProduct = existingProduct.Data.Data;

            bool anyChanges = false;

            if (!string.IsNullOrWhiteSpace(product.Name))
            {
                newProduct.Name = product.Name;
                anyChanges = true;
            }

            if (!string.IsNullOrWhiteSpace(product.Type))
            {
                newProduct.Type = product.Type;
                anyChanges = true;
            }

            if (product.Weight != 0)
            {
                newProduct.Weight = product.Weight;
                anyChanges = true;
            }

            if (product.Price != 0)
            {
                newProduct.Price = product.Price;
                anyChanges = true;
            }

            //if (!string.IsNullOrWhiteSpace(product.BrandName))
            //{
            //    newProduct.BrandName = product.BrandName;
            //    anyChanges = true;
            //}

            if (!string.IsNullOrWhiteSpace(product.Sku))
            {
                newProduct.Sku = product.Sku;
                anyChanges = true;
            }

            if (product.InventoryLevel != -1)
            {
                newProduct.InventoryLevel = product.InventoryLevel;
                anyChanges = true;
            }

            if (!anyChanges)
            {
                return new OperationResult<MetaData<Product>>
                {
                    Success = false,
                    Message = "No changes were provided to update the product",
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Errors = new Error { Errors = new { Error = "No changes were provided to update the product" } }
                };
            }

            var res = await client.PutAsync(fullUrl, new StringContent(JsonSerializer.Serialize(newProduct), Encoding.UTF8, "application/json"));

            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadFromJsonAsync<MetaData<Product>>();
                return new OperationResult<MetaData<Product>>
                {
                    Success = true,
                    Message = "Product updated successfully",
                    StatusCode = (int)HttpStatusCode.OK,
                    Data = content
                };
            }
            else if (res.StatusCode == HttpStatusCode.Created)
            {
                // Returns nothing
                return new OperationResult<MetaData<Product>>
                {
                    Success = true,
                    Message = "Product created successfully",
                    StatusCode = (int)HttpStatusCode.Created
                };
            }
            else if (res.StatusCode == HttpStatusCode.MultiStatus)
            {
                var content = await res.Content.ReadFromJsonAsync<MetaData<Product>>();
                return new OperationResult<MetaData<Product>>
                {
                    Success = false,
                    Message = "The request was partially successful. Some operations may have failed.",
                    StatusCode = (int)HttpStatusCode.MultiStatus,
                    Data = content
                };
            }
            else if (res.StatusCode == HttpStatusCode.NotFound)
            {
                var content = await res.Content.ReadFromJsonAsync<Error>();
                return new OperationResult<MetaData<Product>>
                {
                    Success = false,
                    Message = content.Title,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Errors = content
                };
            }
            else if (res.StatusCode == HttpStatusCode.Conflict)
            {
                var content = await res.Content.ReadFromJsonAsync<Error>();
                return new OperationResult<MetaData<Product>>
                {
                    Success = false,
                    Message = content.Title,
                    StatusCode = (int)HttpStatusCode.Conflict,
                    Errors = content
                };
            } else
            {
                // 422
                var content = await res.Content.ReadFromJsonAsync<Error>();
                return new OperationResult<MetaData<Product>>
                {
                    Success = false,
                    Message = content.Title,
                    StatusCode = (int)HttpStatusCode.UnprocessableEntity,
                    Errors = content
                };
            }
        }
    }
}
