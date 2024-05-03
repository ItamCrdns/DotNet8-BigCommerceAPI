using BeachCommerce.Common;
using BeachCommerce.Dto;
using BeachCommerce.Models;

namespace BeachCommerce.Abstractions
{
    public interface IProductRepository
    {
        Task<MetaData<ProductDto[]>> GetAllProducts(int? page = 1, int? limit = 50); // 200
        Task<OperationResult<MetaData<Product>>> CreateProduct(NewProduct product); // 200, 207, 409, 422
        Task<OperationResult<MetaData<Image[]>>> GetProductImages(int productId, int? page = 1, int? limit = 50); // 200, 204, 404
        Task<OperationResult<MetaData<Image>>> CreateProductImage(int productId, IFormFile image); // 200, 400, 404 and 422
        Task<OperationResult<MetaData<ProductDto>>> GetAProduct(int productId); // 200 and 404
        Task<OperationResult<bool>> DeleteAProduct(int productId); // 204
        Task<OperationResult<bool>> DeleteProductImage(int productId, int imageId); // 204
        Task<OperationResult<MetaData<Product>>> UpdateAProduct(int productId, ProductDto product); // 200, 201, 207, 404, 409, 422
    }
}
