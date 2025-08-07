using Microsoft.EntityFrameworkCore;
using HotelBookingSystem.Models.Auth;
using Microsoft.EntityFrameworkCore.Design;

namespace HotelBookingSystem.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }
        
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Seed default admin and user
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = "admin123", Role = "Admin" },
                new User { Id = 2, Username = "user1", Password = "user123", Role = "User" }
            );
        }
    }
}