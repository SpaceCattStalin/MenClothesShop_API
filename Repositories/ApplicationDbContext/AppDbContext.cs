using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repositories.Models;

namespace Repositories.ApplicationDbContext
{

    //public class AppDbContext : IdentityDbContext<User>
    public class AppDbContext : DbContext
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

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<CartItem> CartItem { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> Items { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Cart>()
                .HasOne(e => e.User)
                    .WithOne(u => u.Cart)
                        .HasForeignKey<Cart>(c => c.UserId);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                    .WithMany(c => c.Items)
                        .HasForeignKey(ci => ci.CartId);

            builder.Entity<CartItem>()
                 .HasOne(ci => ci.ProductVariant)
                     .WithMany()
                         .HasForeignKey(ci => ci.ProductVariantId);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.Size)
                    .WithMany()
                        .HasForeignKey(ci => ci.SizeId);


            builder.Entity<Product>()
                .HasMany<ProductVariant>(e => e.Variants)
                    .WithOne(v => v.MainProduct);

            builder.Entity<Product>()
                .HasOne(e => e.Category)
                    .WithMany(c => c.Products)
                        .HasForeignKey(e => e.CatId);

            builder.Entity<ProductVariant>()
                .HasOne(e => e.MainProduct)
                    .WithMany(p => p.Variants)
                        .HasForeignKey(e => e.MainProductId);

            builder.Entity<ProductVariant>()
                .HasOne(e => e.Color)
                .WithMany(c => c.Variants)
                .HasForeignKey(e => e.ColorCode);

            builder.Entity<ProductImage>()
                .HasOne(e => e.Variant)
                .WithMany(v => v.Images)
                .HasForeignKey(e => e.VariantId);

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

            builder.Entity<Order>()
                .HasOne(e => e.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(e => e.UserId);

            builder.Entity<OrderItem>()
                .HasKey(e => new { e.OrderId, e.ProductVariantId, e.SizeId });

            builder.Entity<OrderItem>()
                .HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId);

            builder.Entity<OrderItem>()
                .HasOne(e => e.ProductVariant)
                .WithMany(pv => pv.OrderItems)
                .HasForeignKey(e => e.ProductVariantId);

            builder.Entity<Message>()
                .HasOne(e => e.Conversation)
                    .WithMany(c => c.Messages)
                        .HasForeignKey(e => e.ConversationId);

            builder.Entity<Conversation>()
                .HasOne(e => e.User)
                    .WithMany(u => u.Conversations)
                        .HasForeignKey(e => e.UserId);
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
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(GetConnectionString("MySQLConnection"), new MySqlServerVersion(new Version("5.2.1")))
                   .UseSeeding(static (context, _) =>
                   {
                       if (!context.Set<Category>().Any())
                       {
                           context.Set<Category>().AddRange(
                               new Category { Id = 1, Name = "Cà Vạt" },
                               new Category { Id = 2, Name = "Túi Đeo Chéo" },
                               new Category { Id = 3, Name = "Áo Thun" }
                           );
                       }
                       context.SaveChanges();

                       if (!context.Set<Product>().Any())
                       {
                           var catTie = context.Set<Category>().First(c => c.Name == "Cà Vạt");
                           var catBag = context.Set<Category>().First(c => c.Name == "Túi Đeo Chéo");
                           var catTshirt = context.Set<Category>().First(c => c.Name == "Áo Thun");

                           context.Set<Product>().AddRange(
                               new Product
                               {
                                   Name = "Cà Vạt Nam Bản Trung Aristino ATI001S0H2",
                                   Price = 199000,
                                   CatId = catTie.Id,
                               },
                               new Product
                               {
                                   Name = "Cà Vạt Nam Lụa Trơn Aristino ATI002S0H3",
                                   Price = 179000,
                                   CatId = catTie.Id,
                               },
                               new Product
                               {
                                   Name = "Cà Vạt Nam Kẻ Sọc Cao Cấp Aristino ATI003S0H1",
                                   Price = 219000,
                                   CatId = catTie.Id,
                               },
                               new Product
                               {
                                   Name = "Cà Vạt Nam Họa Tiết Chấm Bi Aristino ATI004S0H5",
                                   Price = 189000,
                                   CatId = catTie.Id,
                               },
                               new Product
                               {
                                   Name = "Cà Vạt Nam Lụa Hàn Quốc Aristino ATI005S0H7",
                                   Price = 249000,
                                   CatId = catTie.Id,
                               },
                               new Product
                               {
                                   Name = "Cà Vạt Nam Bản Nhỏ Công Sở Aristino ATI006S0H4",
                                   Price = 169000,
                                   CatId = catTie.Id,
                               },
                               new Product
                               {
                                   Name = "Túi Đeo Chéo Nam Canvas Đen Aristino ACB0010S2",
                                   Price = 459000,
                                   CatId = catBag.Id,
                               },
                               new Product
                               {
                                   Name = "Túi Đeo Chéo Nam Da PU Cao Cấp Aristino ACB0020S1",
                                   Price = 589000,
                                   CatId = catBag.Id,
                               },
                               new Product
                               {
                                   Name = "Túi Đeo Chéo Nam Thể Thao Chống Nước Aristino ACB0030S3",
                                   Price = 399000,
                                   CatId = catBag.Id,
                               },
                               new Product
                               {
                                   Name = "Túi Đeo Chéo Nam Phong Cách Hàn Quốc Aristino ACB0040S5",
                                   Price = 499000,
                                   CatId = catBag.Id,
                               },
                               new Product
                               {
                                   Name = "Túi Đeo Chéo Nam Mini Gọn Nhẹ Aristino ACB0050S4",
                                   Price = 349000,
                                   CatId = catBag.Id,
                               },
                               new Product
                               {
                                   Name = "Áo Thun Nam Cổ Tròn Trơn Aristino Basic 1TSS01S",
                                   Price = 259000,
                                   CatId = catTshirt.Id,
                               },
                               new Product
                               {
                                   Name = "Áo Thun Nam Cotton 100% Aristino 1TSS02S",
                                   Price = 299000,
                                   CatId = catTshirt.Id,
                               },
                               new Product
                               {
                                   Name = "Áo Thun Nam In Logo Aristino Sport 1TSS03S",
                                   Price = 279000,
                                   CatId = catTshirt.Id,
                               },
                               new Product
                               {
                                   Name = "Áo Thun Nam Form Slimfit Aristino 1TSS05S",
                                   Price = 289000,
                                   CatId = catTshirt.Id,
                               },
                               new Product
                               {
                                   Name = "Áo Thun Nam Thoáng Khí Aristino Active 1TSS06S",
                                   Price = 319000,
                                   CatId = catTshirt.Id,
                               }, new Product
                               {
                                   Name = "Áo Thun Nam Kẻ Sọc Aristino Trend 1TSS07S",
                                   Price = 269000,
                                   CatId = catTshirt.Id,
                               });
                       }
                       context.SaveChanges();

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
    }
}