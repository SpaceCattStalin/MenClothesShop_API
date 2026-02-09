using Common.Commons;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Repositories.ApplicationDbContext;
using Repositories.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Features
{
    public static class AuthenticationEndpoints
    {
        public record Request(string Email, string Password);
        public record RegisterResult(int userId);
        public static void MapEndpoint(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapPost("register", async (Request request, AppDbContext context) =>
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    if (await context.Users.AnyAsync(u => u.UserName == request.Email))
                    {
                        return ApiResponse.ErrorResult("User already exists", HttpStatusCode.BadRequest);
                    }

                    var user = new User
                    {
                        UserName = request.Email,
                        PasswordHash = HashPassword(request.Password)
                    };

                    context.Users.Add(user);
                    await context.SaveChangesAsync();
                    await transaction.CommitAsync();


                    return ApiResponse<RegisterResult>.SuccessResult(new RegisterResult(user.UserId), HttpStatusCode.Created, "User created successfully");
                }
            });

            routeBuilder.MapPost("login", async (Request request, IConfiguration configuration, AppDbContext context) =>
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == request.Email);

                if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                {
                    return ApiResponse.ErrorResult("Invalid email or password", HttpStatusCode.Unauthorized, ErrorCode.AccessDenied);
                }


                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"]!));

                var crenditials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

                List<Claim> claims =
                [
                    new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                    new(JwtRegisteredClaimNames.Email, user.UserName),
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

        private static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                100000,
                HashAlgorithmName.SHA256,
                32
            );

            byte[] hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            return Convert.ToBase64String(hashBytes);
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);

            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                100000,
                HashAlgorithmName.SHA256,
                32
            );

            for (int i = 0; i < 32; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
