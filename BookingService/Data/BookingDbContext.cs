using Microsoft.EntityFrameworkCore;
using HotelBookingSystem.Models.Booking;
using System.Text.Json;

namespace HotelBookingSystem.Data
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

        public DbSet<BookingFormModel> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure JSON conversion for List<string> properties
            modelBuilder.Entity<BookingFormModel>()
                .Property(b => b.Days)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>());

            modelBuilder.Entity<BookingFormModel>()
                .Property(b => b.Amenities)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>());
        }
    }
}