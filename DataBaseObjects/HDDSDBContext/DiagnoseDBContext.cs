using DataBaseObjects.AlertDB;
using DataBaseObjects.DiagnoseDB;
using DataBaseObjects.RolesDB;
using DataBaseObjects.UsersDB;
using HeartDiseasesDiagnosticExtentions.ResponseExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace DataBaseObjects.HDDSDBContext
{
    public class DiagnoseDBContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<DiagnoseData> DiagnoseData { get; set; }
        public DbSet<DataToStorage> DataToStorage { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<UserRole> Roles { get; set; }
        public DiagnoseDBContext(DbContextOptions<DiagnoseDBContext> options) : base(options)
        {
            ConfigurateContext();
        }
        public DiagnoseDBContext() : base()
        {
            ConfigurateContext();
        }

        private void ConfigurateContext()
        {
            string? connectionString = GetConnectionString();
            Database.SetConnectionString(connectionString);
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            //Database.EnsureDeleted();
            Database.EnsureCreated();
            //Database.Migrate();
        }

        private string? GetConnectionString()
        {
            IConfiguration? config = Database.GetService<IConfiguration>();
            return config?.GetConnectionString("DefaultConnection");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(u => u.Id);
            modelBuilder.Entity<User>().HasMany(u => u.DiagnoseData).WithOne(d => d.User).HasForeignKey(d => d.UserId);
            modelBuilder.Entity<User>().HasMany(x => x.StorageData).WithOne(x => x.User).HasForeignKey(x => x.UserKey);
            modelBuilder.Entity<User>().HasMany(x => x.Alerts).WithOne(x => x.User).HasForeignKey(x => x.UserId);
            modelBuilder.Entity<User>().HasOne(x => x.Role);

            modelBuilder.Entity<DiagnoseData>().HasKey(u => u.Id);
            modelBuilder.Entity<DiagnoseData>().HasOne(d => d.User).WithMany(u => u.DiagnoseData).HasForeignKey(d => d.UserId);
            modelBuilder.Entity<DiagnoseData>().Property(d => d.Result).HasConversion(new EnumToStringConverter<Result>());

            modelBuilder.Entity<DataToStorage>().HasKey(u => u.Id);
            modelBuilder.Entity<DataToStorage>().HasOne(p => p.User).WithMany(x => x.StorageData).HasForeignKey(x => x.UserKey);

            modelBuilder.Entity<Alert>().HasKey(u => u.Id);
            modelBuilder.Entity<Alert>().HasOne(x => x.User).WithMany(x => x.Alerts).HasForeignKey(x => x.UserId);
            modelBuilder.Entity<Alert>().Property(a => a.Level).HasConversion(new EnumToStringConverter<AlertLevel>());
            modelBuilder.Entity<Alert>().Property(a => a.Type).HasConversion(new EnumToStringConverter<AlertType>());

            modelBuilder.Entity<UserRole>().HasKey(r => r.RoleName);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql();
        }
    }
}
