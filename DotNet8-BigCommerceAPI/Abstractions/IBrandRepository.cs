using BeachCommerce.Common;
using BeachCommerce.Dto;
using BeachCommerce.Models;

namespace BeachCommerce.Abstractions
{
    public interface IBrandRepository
    {
        Task<OperationResult<MetaData<Brand>>> CreateBrand(BrandDto brand); // 200, 207, 409, 422
        Task<MetaData<Brand[]>> GetAllBrands(int? page = 1, int? limit = 50);
        Task<OperationResult<BrandDto>> GetABrand(int brandId);
    }
}
