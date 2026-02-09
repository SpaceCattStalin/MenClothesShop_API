
using Amazon.Runtime;
using Amazon.S3;
using API.Background_Tasks;
using API.Features;
using API.Interfaces;
using API.Services;
using API.Services.API.Services;
using Repositories.ApplicationDbContext;
using Services;

namespace MenClothesShop_API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            //builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddDbContext<AppDbContext>();
            //builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<AppDbContext>();

            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IInventoryService, InventoryService>();
            builder.Services.AddScoped<ISizeService, SizeService>();

            builder.Services.AddHostedService<CartExpirationService>();

            var credentials = new BasicAWSCredentials(
                    builder.Configuration.GetValue<string>("MinIO:AccessKey"),
                    builder.Configuration.GetValue<string>("MinIO:SecretKey")
            );

            builder.Services.AddSingleton<IAmazonS3>(sp =>
            {
                var config = new AmazonS3Config
                {
                    ServiceURL = "http://localhost:9000",
                    ForcePathStyle = true,
                };

                return new AmazonS3Client(credentials, config);
            });

            builder.Services.AddScoped<ImageService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                await IdentitySeeder.SeedAsync(scope.ServiceProvider);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            AuthenticationEndpoints.MapEndpoint(app);
            CategoryEndpoint.MapEndpoint(app);
            ProductByCategoryEndpoint.MapEndpoints(app);
            ProductDetailEndpoint.MapEndpoint(app);
            CartEndpoints.MapEndpoint(app);
            //AddToCartEndpoints.MapEndpoint(app);


            app.Run();
        }
    }

    //public static class DatabaseRegistrationExtensions
    //{
    //    public static IServiceCollection RegisterDatabases(this IServiceCollection services, IConfiguration configuration, HashSet<Type> assembyMarkers)
    //    {
    //        var options = configuration.Get<DatabaseSettings>().Databases.ToDictionary(db => db.Name, BuildContextOptions);
    //        var registrationMap = BuildRegistrationMap(options.Keys, assembyMarkers);
    //        var dbContextLookup = registrationMap.ToDictionary(r => r.Key, r => new Func<AppDbContext>(() => new AppDbContext(options[r.Value], registrationMap.Keys)));
    //        services.AddSingleton(dbContextLookup);
    //        services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
    //        return services;
    //    }

    //    private static DbContextOptions BuildContextOptions(DatabaseConfig config) => config.DatabaseType switch
    //    {
    //        DatabaseType.None => throw new ArgumentException("Invalid Dataase Type", nameof(config)),
    //        DatabaseType.SqlServer => new DbContextOptionsBuilder().UseSqlServer(config.ConnectionString).Options,
    //        DatabaseType.MySql => new DbContextOptionsBuilder().UseMySQL(config.ConnectionString).Options,
    //        _ => throw new ArgumentException("Unknow Database Type", nameof(config))
    //    };


    //    private static Dictionary<Type, string> BuildRegistrationMap(IEnumerable<string> databases, IEnumerable<Type> markers)
    //    {
    //        var result = new Dictionary<Type, string>();
    //        foreach (var marker in markers)
    //        {
    //            var entityTypes = GetAssemblyTypes(marker).Where(IsEntityType).ToHashSet();
    //            foreach (var type in entityTypes)
    //            {
    //                var matches = databases.Where(type.FullName.Contains).ToHashSet();
    //                if (matches.IsNullOrEmpty())
    //                {
    //                    throw new ArgumentException("Test1");
    //                }
    //                if (matches.Count > 1)
    //                {
    //                    throw new ArgumentException("Test2");
    //                }
    //                result.Add(type, matches.Single());
    //            }
    //        }
    //        return result;
    //    }

    //    private static Type[] GetAssemblyTypes(Type assemblyMarker) => assemblyMarker.Assembly.GetTypes();

    //    private static readonly Func<Type, bool> IsEntityType = (Type t) => !t.IsInterface && Array.Exists(t.GetInterfaces(), i => i == typeof(IEntity));
    //}

}
