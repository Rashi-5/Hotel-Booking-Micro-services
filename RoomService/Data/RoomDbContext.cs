using Microsoft.EntityFrameworkCore;
using HotelBookingSystem.Models.Room;
using Microsoft.EntityFrameworkCore.Design;

namespace HotelBookingSystem.Data
{
    public class RoomDbContext : DbContext
    {
        public RoomDbContext(DbContextOptions<RoomDbContext> options) : base(options) { }

        public DbSet<RoomCardViewModel> Rooms { get; set; }
    }
}