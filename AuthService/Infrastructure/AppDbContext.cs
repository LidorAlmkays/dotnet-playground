using AuthService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<UserModel> Users { get; set; }
        public DbSet<AuthMethodModel> AuthMethods { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<UserModel>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<AuthMethodModel>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<AuthMethodModel>()
                .HasIndex(a => new { a.UserId, a.Provider })
                .IsUnique();

            modelBuilder.Entity<AuthMethodModel>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuthMethods)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Optional: Remove auth methods when user is deleted
        }
    }
}