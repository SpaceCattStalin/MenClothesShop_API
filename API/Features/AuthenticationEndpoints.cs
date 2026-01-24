using Common.Commons;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Repositories.ApplicationDbContext;
using Repositories.Models;
using System.Security.Claims;
using System.Text;

namespace API.Features
{
    public static class AuthenticationEndpoints
    {
        public record Request(string Email, string Password);
        public record RegisterResult(int userId);
        public static void MapEndpoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapPost("register", async (Request request, AppDbContext context, UserManager<User> manager) =>
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    var user = new User
                    {
                        UserName = request.Email,
                        PasswordHash = request.Password
                    };

                    IdentityResult identityResult = await manager.CreateAsync(user, request.Password);
                    if (!identityResult.Succeeded)
                    {
                        return ApiResponse.ErrorResult("User creation failed", HttpStatusCode.BadRequest);
                    }

                    context.SaveChanges();
                    transaction.Commit();

                    return ApiResponse<RegisterResult>.SuccessResult(new RegisterResult(int.Parse(user.Id)), HttpStatusCode.Created, "User created successfully");
                }
            });

            routeBuilder.MapPost("login", async (Request request, UserManager<User> manager, IConfiguration configuration) =>
            {
                var user = await manager.FindByEmailAsync(request.Email);

                if (user == null || !await manager.CheckPasswordAsync(user, request.Password))
                {
                    return ApiResponse.ErrorResult("Invalid email or password", HttpStatusCode.Unauthorized, ErrorCode.AccessDenied);
                }

                var roles = await manager.GetRolesAsync(user);

                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"]!));

                var crenditials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

                List<Claim> claims =
                [
                    new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new(JwtRegisteredClaimNames.Email, user.UserName),
                    ..roles.Select(r => new Claim(ClaimTypes.Role, r))
                ];

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.Now.AddMinutes(configuration.GetValue<int>("Jwt:ExpirationInMinutes")),
                    Issuer = configuration.GetValue<string>("Jwt:Issuer"),
                    Audience = configuration.GetValue<string>("Jwt:Audience"),
                    SigningCredentials = crenditials
                };

                var tokenHandler = new JsonWebTokenHandler();

                string accessToken = tokenHandler.CreateToken(tokenDescriptor);

                return ApiResponse<string>.SuccessResult(accessToken);
            });
        }
    }
}
