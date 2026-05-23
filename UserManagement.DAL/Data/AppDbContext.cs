using Microsoft.EntityFrameworkCore;
using UserManagement.DAL.Models;

namespace UserManagement.DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.RoleName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(x => x.RoleName)
                    .IsUnique();

                entity.HasData(
                    new Role { Id = 1, RoleName = "Admin" },
                    new Role { Id = 2, RoleName = "User" }
                );
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Username)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(x => x.Username)
                    .IsUnique();

                entity.Property(x => x.Password)
                    .IsRequired();

                entity.Property(x => x.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasOne(x => x.Role)
                    .WithMany(x => x.Users)
                    .HasForeignKey(x => x.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(x => x.Status)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(x => x.IsDeleted)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(x => x.LastActivityAt)
                      .IsRequired(false);

                entity.Property(x => x.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}

