using BeachCommerce.Abstractions;
using BeachCommerce.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BeachCommerce.Endpoints
{
    public static class Users
    {
        public static void RegisterUsersEndpoints(this IEndpointRouteBuilder routeBuilder)
        {
            RouteGroupBuilder users = routeBuilder.MapGroup("api/v1/users");

            users.MapPost("/login", Login);
        }

        public static async Task<Results<Ok<string>, UnauthorizedHttpResult>> Login([FromServices] IUserRepository userRepository, [FromBody] User user)
        {
            var token = userRepository.Login(user);

            if (token != null)
            {
                return TypedResults.Ok(token);
            }
            else
            {
                return TypedResults.Unauthorized();
            }
        }
    }
}
