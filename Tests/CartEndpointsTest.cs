using Common.Commons;
using FluentAssertions;
using MenClothesShop_API;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Repositories.ApplicationDbContext;
using Repositories.Models;
using System.Net.Http.Json;
using static API.Features.CartEndpoints;

namespace API.Tests
{
    [TestFixture]
    public class CartEndpointsTest
    {
        private HttpClient _client;
        private WebApplicationFactory<Program> _factory;

        [SetUp]
        public void Setup()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services
                            .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                        if (descriptor != null)
                            services.Remove(descriptor);

                        // Use unique database name per test
                        services.AddDbContext<AppDbContext>(options =>
                        {
                            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                        });
                    });
                });

            _client = _factory.CreateClient();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up the in-memory database
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureDeleted();
            }

            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task ValidateCart_ReturnsInsufficientStock_WhenQuantityTooHigh()
        {
            // Arrange
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
                SeedDatabase(db);

                // Debug: Check what was actually seeded
                var cartItems = db.CartItem.Include(ci => ci.ProductVariant).ToList();
                Console.WriteLine($"Cart items count: {cartItems.Count}");
                foreach (var item in cartItems)
                {
                    Console.WriteLine($"CartItem: Quantity={item.Quantity}, VariantId={item.ProductVariantId}");
                }
            }

            var userId = 2;

            // Act
            var response = await _client.PostAsync($"/validate-cart?userId={userId}", null);

            // Debug output
            Console.WriteLine($"Response Status: {response.StatusCode}");
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Body: {content}");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);

            var apiResponse = await response.Content
                .ReadFromJsonAsync<ApiResponse<List<ValidationIssue>>>();

            var issues = apiResponse!.Data;

            issues.Should().NotBeEmpty();
        }

        private void SeedDatabase(AppDbContext db)
        {
            var category = new Category { Name = "Clothing", ImgUri = "test.jpg" };
            var color = new Color { Name = "Black", HexCode = "#000000" };
            var size = new Size { Name = "Large", Description = "L" };

            db.Category.Add(category);
            db.Colors.Add(color);
            db.Sizes.Add(size);
            db.SaveChanges();

            var product = new Product
            {
                Name = "Premium Hoodie",
                Price = 50.00m,
                CatId = category.Id,
                TotalQuantity = 100
            };
            db.Products.Add(product);
            db.SaveChanges();

            var variant = new ProductVariant
            {
                MainProductId = product.Id,
                ColorCode = color.Id,
                TotalQuantity = 100
            };
            db.ProductVariants.Add(variant);
            db.SaveChanges();

            db.ProductSizes.Add(new ProductSize
            {
                ProductVariantId = variant.Id,
                SizeId = size.Id,
                Quantity = 10
            });

            var user = new User { UserId = 2, UserName = "testuser", PasswordHash = "abc" };
            db.Users.Add(user);
            db.SaveChanges();

            var cart = new Cart
            {
                UserId = user.UserId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            db.Cart.Add(cart);
            db.SaveChanges();

            db.CartItem.Add(new CartItem
            {
                CartId = cart.Id,
                ProductVariantId = variant.Id,
                SizeId = size.Id,
                Quantity = 50
            });

            db.SaveChanges();
        }
    }
}
