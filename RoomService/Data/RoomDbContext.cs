using Microsoft.EntityFrameworkCore;
using HotelBookingSystem.Models.Room;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace HotelBookingSystem.Data
{
    public class RoomDbContext : DbContext
    {
        public RoomDbContext(DbContextOptions<RoomDbContext> options) : base(options) { }

        public DbSet<RoomCardViewModel> Rooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure JSON conversion for List<string> properties
            modelBuilder.Entity<RoomCardViewModel>()
                .Property(r => r.Amenities)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>());
        }
    }
}