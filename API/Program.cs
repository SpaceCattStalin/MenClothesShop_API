
using Common.Commons;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Repositories.ApplicationDbContext;

namespace MenClothesShop_API
{
    public class Program
    {
        public static void Main(string[] args)
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

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }

    public static class DatabaseRegistrationExtensions
    {
        public static IServiceCollection RegisterDatabases(this IServiceCollection services, IConfiguration configuration, HashSet<Type> assembyMarkers)
        {
            var options = configuration.Get<DatabaseSettings>().Databases.ToDictionary(db => db.Name, BuildContextOptions);
            var registrationMap = BuildRegistrationMap(options.Keys, assembyMarkers);
            var dbContextLookup = registrationMap.ToDictionary(r => r.Key, r => new Func<AppDbContext>(() => new AppDbContext(options[r.Value], registrationMap.Keys)));
            services.AddSingleton(dbContextLookup);
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            return services;
        }

        private static DbContextOptions BuildContextOptions(DatabaseConfig config) => config.DatabaseType switch
        {
            DatabaseType.None => throw new ArgumentException("Invalid Dataase Type", nameof(config)),
            DatabaseType.SqlServer => new DbContextOptionsBuilder().UseSqlServer(config.ConnectionString).Options,
            DatabaseType.MySql => new DbContextOptionsBuilder().UseMySQL(config.ConnectionString).Options,
            _ => throw new ArgumentException("Unknow Database Type", nameof(config))
        };


        private static Dictionary<Type, string> BuildRegistrationMap(IEnumerable<string> databases, IEnumerable<Type> markers)
        {
            var result = new Dictionary<Type, string>();
            foreach (var marker in markers)
            {
                var entityTypes = GetAssemblyTypes(marker).Where(IsEntityType).ToHashSet();
                foreach (var type in entityTypes)
                {
                    var matches = databases.Where(type.FullName.Contains).ToHashSet();
                    if (matches.IsNullOrEmpty())
                    {
                        throw new ArgumentException("Test1");
                    }
                    if (matches.Count > 1)
                    {
                        throw new ArgumentException("Test2");
                    }
                    result.Add(type, matches.Single());
                }
            }
            return result;
        }

        private static Type[] GetAssemblyTypes(Type assemblyMarker) => assemblyMarker.Assembly.GetTypes();

        private static readonly Func<Type, bool> IsEntityType = (Type t) => !t.IsInterface && Array.Exists(t.GetInterfaces(), i => i == typeof(IEntity));
    }

}
