using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace WebApiTutorial.Models
{
    public interface IPlacesContext
    {
        DbSet<Places> Places { get; set; }
    }
    public partial class PlacesContext : DbContext, IPlacesContext
    {
        private readonly string _connectionString;
        public PlacesContext()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
             .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
             .AddJsonFile("appsettings.json")
             .Build();
            _connectionString = configuration.GetConnectionString("PlacesDatabase");
        }

        public PlacesContext(DbContextOptions<PlacesContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Places> Places { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Places>(entity =>
            {
                //entity.HasNoKey();
                entity.HasKey(e => e.Id).HasName("PK_Id");

                entity.Property(e => e.City).IsUnicode(false);

                entity.Property(e => e.Ew).IsUnicode(false);

                entity.Property(e => e.Ns).IsUnicode(false);

                entity.Property(e => e.State).IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
