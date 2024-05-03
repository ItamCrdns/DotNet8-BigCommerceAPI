using BeachCommerce.Abstractions;
using BeachCommerce.Common;
using BeachCommerce.Dto;
using BeachCommerce.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace BeachCommerce.Endpoints
{
    public static class Products
    {
        public static void RegisterProductsEndpoints(this IEndpointRouteBuilder routeBuilder)
        {
            RouteGroupBuilder products = routeBuilder.MapGroup("api/v1/products");

            products.MapGet("/all", GetAllProducts).RequireAuthorization();
            products.MapGet("/{productId}", GetAProduct).RequireAuthorization();
            products.MapGet("/{productId}/images", GetProductImages).RequireAuthorization();
            products.MapPost("/create", CreateProduct).RequireAuthorization();
            products.MapPost("/{productId}/images", CreateProductImage).DisableAntiforgery().RequireAuthorization();
            products.MapDelete("/{productId}", DeleteAProduct).RequireAuthorization();
            products.MapDelete("/{productId}/images/{imageId}", DeleteProductImage).RequireAuthorization();
            products.MapPut("/{productId}", UpdateAProduct).RequireAuthorization();
        }

        public static async Task<Ok<MetaData<ProductDto[]>>> GetAllProducts([FromServices] IProductRepository productRepository, [FromQuery] int? page, [FromQuery] int? limit)
        {
            var res = await productRepository.GetAllProducts(page, limit);

            return TypedResults.Ok(res);
        }

        public static async Task<
                Results<Ok<OperationResult<MetaData<Product>>>,
                BadRequest<OperationResult<MetaData<Product>>>,
                Conflict<OperationResult<MetaData<Product>>>,
                UnprocessableEntity<OperationResult<MetaData<Product>>>>>
            CreateProduct([FromServices] IProductRepository productRepository, [FromBody] NewProduct product)
        {
            var res = await productRepository.CreateProduct(product);

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

        public static async Task<
                Results<Ok<OperationResult<MetaData<Image[]>>>,
                NoContent,
                NotFound<OperationResult<MetaData<Image[]>>>>>
            GetProductImages([FromServices] IProductRepository productRepository, [FromRoute] int productId, [FromQuery] int? page, [FromQuery] int? limit)
        {
            var res = await productRepository.GetProductImages(productId, page, limit);

            if (res.StatusCode == 200)
            {
                return TypedResults.Ok(res);
            }
            else if (res.StatusCode == 204)
            {
                return TypedResults.NoContent(); 
            }
            else
            {
                return TypedResults.NotFound(res);
            }
        }

        public static async Task<
            Results<Ok<OperationResult<MetaData<ProductDto>>>,
            NotFound<OperationResult<MetaData<ProductDto>>>>>
            GetAProduct([FromServices] IProductRepository productRepository, int productId)
        {
            var res = await productRepository.GetAProduct(productId);

            if (res.Success)
            {
                return TypedResults.Ok(res);
            }
            else
            {
                return TypedResults.NotFound(res);
            }
        }

        public static async Task<
            Results<Ok<OperationResult<MetaData<Image>>>,
            NotFound<OperationResult<MetaData<Image>>>,
            BadRequest<OperationResult<MetaData<Image>>>>>
            CreateProductImage([FromServices] IProductRepository productRepository, [FromRoute] int productId, [FromForm] IFormFile image)
        {
            var res = await productRepository.CreateProductImage(productId, image);

            if (res.StatusCode == 200)
            {
                return TypedResults.Ok(res);
            }
            else if (res.StatusCode == 400)
            {
                return TypedResults.BadRequest(res);
            }
            else if (res.StatusCode == 404)
            {
                return TypedResults.NotFound(res);
            }
            else
            {
                return TypedResults.BadRequest(res);
            }   
        }

        public static async Task<
            Results<Ok<OperationResult<bool>>,
            NotFound<OperationResult<bool>>>>
            DeleteAProduct([FromServices] IProductRepository productRepository, [FromRoute] int productId)
        {
            var res = await productRepository.DeleteAProduct(productId);

            if (res.Success)
            {
                return TypedResults.Ok(res);
            }
            else
            {
                return TypedResults.NotFound(res);
            }
        }

        public static async Task<
            Results<Ok<OperationResult<bool>>,
            NotFound<OperationResult<bool>>,
            NoContent,
            BadRequest<OperationResult<bool>>>>
            DeleteProductImage([FromServices] IProductRepository productRepository, [FromRoute] int productId, [FromRoute] int imageId)
        {
            var res = await productRepository.DeleteProductImage(productId, imageId);

            if (res.StatusCode == 200)
            {
                return TypedResults.Ok(res);
            }
            else if (res.StatusCode == 404)
            {
                return TypedResults.NotFound(res);
            }
            else if (res.StatusCode == 204)
            {
                return TypedResults.NoContent();
            } else
            {
                return TypedResults.BadRequest(res);
            }
        }

        public static async Task<
            Results<Ok<OperationResult<MetaData<Product>>>,
            Created<OperationResult<MetaData<Product>>>,
            BadRequest<OperationResult<MetaData<Product>>>,
            NotFound<OperationResult<MetaData<Product>>>,
            Conflict<OperationResult<MetaData<Product>>>,
            UnprocessableEntity<OperationResult<MetaData<Product>>>
            >>
            UpdateAProduct([FromServices] IProductRepository productRepository, [FromRoute] int productId, [FromBody] ProductDto product)
        {
            var res = await productRepository.UpdateAProduct(productId, product);

            if (res.StatusCode == 200)
            {
                return TypedResults.Ok(res);
            }
            else if (res.StatusCode == 201)
            {
                string location = $"/api/v1/products/{productId}";
                return TypedResults.Created(location, res);
            }
            else if (res.StatusCode == 400)
            {
                return TypedResults.BadRequest(res);
            }
            else if (res.StatusCode == 404)
            {
                return TypedResults.NotFound(res);
            }
            else if (res.StatusCode == 409)
            {
                return TypedResults.Conflict(res);
            }
            else if (res.StatusCode == 422)
            {
                return TypedResults.UnprocessableEntity(res);
            }
            else
            {
                return TypedResults.BadRequest(res);
            }
        }
    }
}
