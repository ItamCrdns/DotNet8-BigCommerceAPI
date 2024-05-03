using BeachCommerce.Abstractions;
using BeachCommerce.Common;
using BeachCommerce.Dto;
using BeachCommerce.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BeachCommerce.Endpoints
{
    public static class Brands
    {
        public static void RegisterBrandsEndpoints(this IEndpointRouteBuilder routeBuilder)
        {
            RouteGroupBuilder brands = routeBuilder.MapGroup("api/v1/brands");

            brands.MapPost("/create", CreateBrand);
            brands.MapGet("/all", GetAllBrands);
            brands.MapGet("/{brandId}", GetABrand).RequireAuthorization();
        }

        public static async Task<
        Results<Ok<OperationResult<MetaData<Brand>>>,
        BadRequest<OperationResult<MetaData<Brand>>>,
        Conflict<OperationResult<MetaData<Brand>>>,
        UnprocessableEntity<OperationResult<MetaData<Brand>>>>>
            CreateBrand([FromServices] IBrandRepository brandRepository, [FromBody] BrandDto brand)
        {
            var res = await brandRepository.CreateBrand(brand);

            if (res.StatusCode == (int)HttpStatusCode.OK)
            {
                return TypedResults.Ok(res);
            }
            else if (res.StatusCode == (int)HttpStatusCode.MultiStatus)
            {
                return TypedResults.BadRequest(res);
            }
            else if (res.StatusCode == (int)HttpStatusCode.Conflict)
            {
                return TypedResults.Conflict(res);
            }
            else if (res.StatusCode == (int)HttpStatusCode.UnprocessableEntity)
            {
                return TypedResults.UnprocessableEntity(res);
            }
            else
            {
                return TypedResults.BadRequest(res);
            }
        }

        public static async Task<Ok<MetaData<Brand[]>>> GetAllBrands([FromServices] IBrandRepository brandRepository, [FromQuery] int? page, [FromQuery] int? limit)
        {
            var res = await brandRepository.GetAllBrands(page, limit);

            return TypedResults.Ok(res);
        }

        public static async Task<
            Results<Ok<OperationResult<BrandDto>>,
            NotFound<OperationResult<BrandDto>>>>
            GetABrand([FromServices] IBrandRepository brandRepository, [FromRoute] int brandId)
        {
            var res = await brandRepository.GetABrand(brandId);

            if (res.Success)
            {
                return TypedResults.Ok(res);
            }
            else
            {
                return TypedResults.NotFound(res);
            }
        }
    }
}
