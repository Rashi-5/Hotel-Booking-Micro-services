using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using HotelBookingSystem.Data;

namespace HotelBookingSystem.Data
{
    public class RoomDbContextFactory : IDesignTimeDbContextFactory<RoomDbContext>
    {
        public RoomDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<RoomDbContext>();
            optionsBuilder.UseSqlite("Data Source=rooms.db");

            return new RoomDbContext(optionsBuilder.Options);
        }
    }
}
