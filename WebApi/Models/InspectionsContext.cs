using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace WebApi.Models
{
    public interface IInspectionsContext
    {
        DbSet<Inspections> Inspections { get; set; }
    }
    public partial class InspectionsContext : DbContext, IInspectionsContext
    {
        private readonly string _connectionString;
        public InspectionsContext()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();
            _connectionString = configuration.GetConnectionString("InspectionsDatabase");
        }

        public InspectionsContext(DbContextOptions<InspectionsContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Inspections> Inspections { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Inspections>(entity =>
            {
                //entity.HasNoKey();
                entity.HasKey(e => e.PermitNumber).HasName("PK_PermitNumber");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
