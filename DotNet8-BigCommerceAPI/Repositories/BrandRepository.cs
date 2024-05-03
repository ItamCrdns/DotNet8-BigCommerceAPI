using BeachCommerce.Abstractions;
using BeachCommerce.Common;
using BeachCommerce.Dto;
using BeachCommerce.Models;
using System.Net;

namespace BeachCommerce.Repositories
{
    public class BrandRepository(IHttpClientFactory httpClientFactory) : IBrandRepository
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        public async Task<OperationResult<MetaData<Brand>>> CreateBrand(BrandDto brand)
        {
            var client = _httpClientFactory.CreateClient("BigCommerce");

            if (string.IsNullOrWhiteSpace(brand.Name))
            {
                return new OperationResult<MetaData<Brand>>
                {
                    Success = false,
                    Message = "Brand name is required",
                    StatusCode = (int)HttpStatusCode.BadRequest,
                };
            }

            string fullUrl = client.BaseAddress + "/catalog/brands";

            var res =  await client.PostAsJsonAsync(fullUrl, brand);

            if (res.StatusCode == HttpStatusCode.OK)
            {
                var content = await res.Content.ReadFromJsonAsync<MetaData<Brand>>();
                return new OperationResult<MetaData<Brand>>
                {
                    Success = true,
                    Data = content,
                    Message = "Brand created successfully",
                    StatusCode = (int)HttpStatusCode.OK,
                };
            }
            else if (res.StatusCode == HttpStatusCode.MultiStatus)
            {
                var content = await res.Content.ReadFromJsonAsync<MetaData<Brand>>();
                return new OperationResult<MetaData<Brand>>
                {
                    Success = false,
                    Data = content,
                    Message = "The request was partially successful. Some operations may have failed.",
                    StatusCode = (int)HttpStatusCode.MultiStatus,
                };
            }
            else if (res.StatusCode == HttpStatusCode.Conflict)
            {
                var content = await res.Content.ReadFromJsonAsync<Error>();
                return new OperationResult<MetaData<Brand>>
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
                return new OperationResult<MetaData<Brand>>
                {
                    Success = false,
                    Message = content.Title,
                    StatusCode = (int)HttpStatusCode.UnprocessableEntity,
                    Errors = content
                };
            }
        }

        public async Task<OperationResult<BrandDto>> GetABrand(int brandId)
        {
            var client = _httpClientFactory.CreateClient("BigCommerce");

            string fullUrl = client.BaseAddress + $"/catalog/brands/{brandId}";

            var res = await client.GetAsync(fullUrl);

            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadFromJsonAsync<MetaData<Brand>>();
                return new OperationResult<BrandDto>
                {
                    Success = true,
                    Data = new BrandDto
                    {
                        Name = content.Data.Name
                    },
                    Message = "Brand retrieved successfully",
                    StatusCode = (int)HttpStatusCode.OK
                };
            } else
            {
                var content = await res.Content.ReadFromJsonAsync<Error>();
                return new OperationResult<BrandDto>
                {
                    Success = false,
                    Message = content.Title,
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Errors = content
                };
            }
        }

        public async Task<MetaData<Brand[]>> GetAllBrands(int? page = 1, int? limit = 50)
        {
            var client = _httpClientFactory.CreateClient("BigCommerce");

            string fullUrl = client.BaseAddress + $"/catalog/brands?page={page}&limit={limit}";

            var res = await client.GetAsync(fullUrl);

            if (res.StatusCode == HttpStatusCode.OK)
            {
                var content = await res.Content.ReadFromJsonAsync<MetaData<Brand[]>>();
                return content;
            }
            else
            {
                return new MetaData<Brand[]>
                {
                    Data = null,
                    Meta = null
                };
            }
        }
    }
}
