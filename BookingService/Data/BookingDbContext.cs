using Microsoft.EntityFrameworkCore;
using HotelBookingSystem.Models.Booking;

namespace HotelBookingSystem.Data
{
    public class BookingDbContext : DbContext
    {
        public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

        public DbSet<BookingFormModel> Bookings { get; set; }
    }
}