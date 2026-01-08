using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repositories.Models;

namespace Repositories.ApplicationDbContext
{

    public class AppDbContext : DbContext
    {
        private readonly HashSet<Type> entityTypes;

        public AppDbContext(DbContextOptions options, IEnumerable<Type> entityTypes) : base(options)
        {
            this.entityTypes = entityTypes.ToHashSet();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var entityType in entityTypes)
            {
                modelBuilder.Entity(entityType);
            }
        }

        //public DbSet<User> Users { get; set; }

        //public static string GetConnectionString(string connectionStringName)
        //{
        //    var config = new ConfigurationBuilder()
        //        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        //        .AddJsonFile("appsettings.Development.json")
        //        .Build();

        //    string connectionString = config.GetConnectionString(connectionStringName);
        //    return connectionString;
        //}

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //    => optionsBuilder.UseSqlServer(GetConnectionString("DefaultConnection"));

    }
}
