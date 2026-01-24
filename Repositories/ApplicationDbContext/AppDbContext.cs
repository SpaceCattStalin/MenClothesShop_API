using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repositories.Models;

namespace Repositories.ApplicationDbContext
{

    public class AppDbContext : IdentityDbContext<User>
    {
        //private readonly HashSet<Type> entityTypes;

        //public AppDbContext(DbContextOptions options, IEnumerable<Type> entityTypes) : base(options)
        //{
        //    this.entityTypes = entityTypes.ToHashSet();
        //}

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //    foreach (var entityType in entityTypes)
        //    {
        //        modelBuilder.Entity(entityType);
        //    }
        //}
        public AppDbContext()
        {

        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        //public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Size> Sizes { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>()
                .HasMany<ProductVariant>(e => e.Variants)
                    .WithOne(v => v.MainProduct);

            builder.Entity<ProductVariant>()
                .HasOne(e => e.MainProduct)
                    .WithMany(p => p.Variants)
                        .HasForeignKey(e => e.MainProductId);

            builder.Entity<ProductVariant>()
                .HasOne(e => e.Color)
                .WithMany(c => c.Variants)
                .HasForeignKey(e => e.ColorCode);

            builder.Entity<ProductVariant>()
                .HasMany(e => e.Sizes)
                .WithOne(s => s.ProductVariant);


            builder.Entity<ProductSize>()
                .HasKey(ps => new { ps.ProductVariantId, ps.SizeId });

            builder.Entity<ProductSize>()
                .HasOne(ps => ps.ProductVariant)
                .WithMany(pv => pv.Sizes)
                .HasForeignKey(ps => ps.ProductVariantId);

            builder.Entity<ProductSize>()
                .HasOne(ps => ps.Size)
                .WithMany()
                .HasForeignKey(ps => ps.SizeId);
        }

        public static string GetConnectionString(string connectionStringName)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.Development.json")
                .Build();

            string connectionString = config.GetConnectionString(connectionStringName);
            return connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
              //=> optionsBuilder.UseSqlServer(GetConnectionString("DefaultConnection"));
              => optionsBuilder.UseMySql(GetConnectionString("MySQLConnection"), new MySqlServerVersion(new Version("5.2.1")))
                    .UseSeeding(static (context, _) =>
                    {

                        if (!context.Set<Product>().Any())
                        {
                            context.Set<Product>().AddRange(
                                new Product
                                {
                                    Name = "Cà Vạt Nam Bản Trung Aristino ATI001S0H2",
                                    Price = 199000
                                },
                                new Product
                                {
                                    Name = "Cà Vạt Nam Lụa Trơn Aristino ATI002S0H3",
                                    Price = 179000
                                },
                                new Product
                                {
                                    Name = "Cà Vạt Nam Kẻ Sọc Cao Cấp Aristino ATI003S0H1",
                                    Price = 219000
                                },
                                new Product
                                {
                                    Name = "Cà Vạt Nam Họa Tiết Chấm Bi Aristino ATI004S0H5",
                                    Price = 189000
                                },
                                new Product
                                {
                                    Name = "Cà Vạt Nam Lụa Hàn Quốc Aristino ATI005S0H7",
                                    Price = 249000
                                },
                                new Product
                                {
                                    Name = "Cà Vạt Nam Bản Nhỏ Công Sở Aristino ATI006S0H4",
                                    Price = 169000
                                },
                                new Product
                                {
                                    Name = "Túi Đeo Chéo Nam Canvas Đen Aristino ACB0010S2",
                                    Price = 459000
                                },
                                new Product
                                {
                                    Name = "Túi Đeo Chéo Nam Da PU Cao Cấp Aristino ACB0020S1",
                                    Price = 589000
                                },
                                new Product
                                {
                                    Name = "Túi Đeo Chéo Nam Thể Thao Chống Nước Aristino ACB0030S3",
                                    Price = 399000
                                },
                                new Product
                                {
                                    Name = "Túi Đeo Chéo Nam Phong Cách Hàn Quốc Aristino ACB0040S5",
                                    Price = 499000
                                },
                                new Product
                                {
                                    Name = "Túi Đeo Chéo Nam Mini Gọn Nhẹ Aristino ACB0050S4",
                                    Price = 349000
                                },
                                new Product
                                {
                                    Name = "Áo Thun Nam Cổ Tròn Trơn Aristino Basic 1TSS01S",
                                    Price = 259000
                                },
                                new Product
                                {
                                    Name = "Áo Thun Nam Cổ Tròn Trơn Aristino Basic 1TSS01S",
                                    Price = 259000
                                },
                                new Product
                                {
                                    Name = "Áo Thun Nam Cổ Tròn Trơn Aristino Basic 1TSS01S",
                                    Price = 259000
                                },
                                new Product
                                {
                                    Name = "Áo Thun Nam Cotton 100% Aristino 1TSS02S",
                                    Price = 299000
                                },
                                new Product
                                {
                                    Name = "Áo Thun Nam In Logo Aristino Sport 1TSS03S",
                                    Price = 279000
                                },
                                new Product
                                {
                                    Name = "Áo Thun Nam Form Slimfit Aristino 1TSS05S",
                                    Price = 289000
                                },
                                new Product
                                {
                                    Name = "Áo Thun Nam Thoáng Khí Aristino Active 1TSS06S",
                                    Price = 319000
                                }, new Product
                                {
                                    Name = "Áo Thun Nam Kẻ Sọc Aristino Trend 1TSS07S",
                                    Price = 269000
                                });
                        }

                        if (!context.Set<Color>().Any())
                        {
                            context.Set<Color>().AddRange(
                                new Color { Name = "Đen", HexCode = "#000000" },
                                new Color { Name = "Trắng", HexCode = "#FFFFFF" },
                                new Color { Name = "Đỏ", HexCode = "#FF0000" },
                                new Color { Name = "Xanh dương", HexCode = "#0000FF" },
                                new Color { Name = "Xanh lá", HexCode = "#008000" },
                                new Color { Name = "Xanh navy", HexCode = "#000080" },
                                new Color { Name = "Xám", HexCode = "#808080" },
                                new Color { Name = "Nâu", HexCode = "#8B4513" }
                            );
                        }

                        if (!context.Set<Size>().Any())
                        {
                            context.Set<Size>().AddRange(
                                new Size { Name = "SIZE0", Description = "Không áp dụng kích thước" },

                                new Size { Name = "S", Description = "Size áo" },
                                new Size { Name = "M", Description = "Size áo" },
                                new Size { Name = "L", Description = "Size áo" },
                                new Size { Name = "XL", Description = "Size áo" },
                                new Size { Name = "XXL", Description = "Size áo" },

                                new Size { Name = "39", Description = "Cỡ giày 39" },
                                new Size { Name = "40", Description = "Cỡ giày 40" },
                                new Size { Name = "41", Description = "Cỡ giày 41" },
                                new Size { Name = "42", Description = "Cỡ giày 42" },
                                new Size { Name = "43", Description = "Cỡ giày 43" }
                            );
                        }

                        context.SaveChanges();


                        if (!context.Set<ProductVariant>().Any())
                        {
                            var black = context.Set<Color>().First(c => c.Name == "Đen");
                            var white = context.Set<Color>().First(c => c.Name == "Trắng");

                            var size0 = context.Set<Size>().First(s => s.Name == "SIZE0");
                            var s = context.Set<Size>().First(s => s.Name == "S");
                            var m = context.Set<Size>().First(s => s.Name == "M");
                            var l = context.Set<Size>().First(s => s.Name == "L");
                            var xl = context.Set<Size>().First(s => s.Name == "XL");

                            var tie = context.Set<Product>().First(p => p.Name.Contains("Cà Vạt"));
                            var tshirt = context.Set<Product>().First(p => p.Name.Contains("Áo Thun"));
                            var bag = context.Set<Product>().First(p => p.Name.Contains("Túi Đeo Chéo"));

                            context.Set<ProductVariant>().AddRange(

                                // Tie - no size
                                new ProductVariant
                                {
                                    MainProductId = tie.Id,
                                    ColorCode = black.Id,
                                    TotalQuantity = 50,
                                    Sizes = new List<ProductSize>
                                    {
                                        new ProductSize { SizeId = size0.Id }
                                    }
                                },

                                // T-shirt - Black
                                new ProductVariant
                                {
                                    MainProductId = tshirt.Id,
                                    ColorCode = black.Id,
                                    TotalQuantity = 100,
                                    Sizes = new List<ProductSize>
                                    {
                                        new ProductSize { SizeId = s.Id },
                                        new ProductSize { SizeId = m.Id },
                                        new ProductSize { SizeId = l.Id },
                                        new ProductSize { SizeId = xl.Id }
                                    }
                                },

                                // T-shirt - White
                                new ProductVariant
                                {
                                    MainProductId = tshirt.Id,
                                    ColorCode = white.Id,
                                    TotalQuantity = 80,
                                    Sizes = new List<ProductSize>
                                    {
                                        new ProductSize { SizeId = s.Id },
                                        new ProductSize { SizeId = m.Id },
                                        new ProductSize { SizeId = l.Id }
                                    }
                                },

                                // Bag - Black
                                new ProductVariant
                                {
                                    MainProductId = bag.Id,
                                    ColorCode = black.Id,
                                    TotalQuantity = 30,
                                    Sizes = new List<ProductSize>
                                    {
                                        new ProductSize { SizeId = size0.Id }
                                    }
                                }
                            );
                        }

                        context.SaveChanges();
                    })
                    .UseAsyncSeeding(async (context, _, cancellationToken) =>
                    {

                    });
    }
}