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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            // Suppress pending model changes warning for Azure deployment
            optionsBuilder.ConfigureWarnings(warnings => 
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the entity
            modelBuilder.Entity<RoomCardViewModel>(entity =>
            {
                // Explicitly configure Id as SQL Server identity column
                entity.Property(e => e.Id)
                    .UseIdentityColumn()
                    .ValueGeneratedOnAdd();

                // Configure JSON conversion for List<string> properties
                entity.Property(r => r.Amenities)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>());
            });
        }
    }
}